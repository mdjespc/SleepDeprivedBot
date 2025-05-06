using Discord;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DiscordBot.Services{
    public class MongoDbService : IMongoDbService{
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database; 
        //private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public MongoDbService(IConfiguration config){
            _config = config;
            // _logger = logger;

            var connectionString = _config["MONGODB:CONNECTION_STRING"] ?? throw new Exception("Missing MongoDB connection string.");
            var client = new MongoClient(connectionString);
            _client = client;
            _database = _client.GetDatabase("discordbot");
            //_logger.LogInformation("Database connection successful.");
            Console.WriteLine("Database connection successful.");
        }

        //Instance fields of Collection type
        public IMongoCollection<GuildSettingsModel> Guilds => _database.GetCollection<GuildSettingsModel>("guilds");
        public IMongoCollection<GuildUserModel> GuildUsers => _database.GetCollection<GuildUserModel>("users");
        public IMongoCollection<WarningModel> Warnings => _database.GetCollection<WarningModel>("warnings");

        //Guild Ops
        public async Task SetGuildSettingsAsync(ulong guildId, string key, string value){
            var filter = Builders<GuildSettingsModel>.Filter.Eq(_ => _.GuildId, guildId);

            //Create the update based on the setting key.
            var update = key switch
            {
                "language" => Builders<GuildSettingsModel>.Update.Set(_ => _.Language, value),
                "prefix" => Builders<GuildSettingsModel>.Update.Set(_ => _.Prefix, value),
                "welcomeChannel" => Builders<GuildSettingsModel>.Update.Set(_ => _.WelcomeChannel, value),
                "welcomeMessage" => Builders<GuildSettingsModel>.Update.Set(_ => _.WelcomeMessage, value),
                "modlog" => Builders<GuildSettingsModel>.Update.Set(_ => _.Modlog, value),
                _ => throw new ArgumentException($"Unknown setting key: {key}")
            };

            try{
                await Guilds.UpdateOneAsync(filter, update);
            }catch(Exception e){
                Console.WriteLine($"`discordbot.guilds`: There was an error while attempting to update the Guild collection.\nFilter: {filter}\nUpdate pushed:{update}\nException:{e}");
                return;
            }
            Console.WriteLine($"`discordbot.guilds`: Guild collection has been updated.\nFilter:{filter}\nUpdate:{update}");
        }

        //Check guild settings — or initialize them if non-existent 
        public async Task<GuildSettingsModel> GetGuildSettingsAsync(ulong guildId){
            var settings = await Guilds.Find(_ => _.GuildId == guildId).FirstOrDefaultAsync();

            if (settings == null){
                settings = new GuildSettingsModel{GuildId = guildId};
                await Guilds.InsertOneAsync(settings);

                Console.WriteLine($"discordbot.guilds: Guild Settings for Guild {guildId} were not found in the database. A new document for this guild has been added.");
            }

            return settings;
        }

        // User Operations
        public async Task CreateGuildUserAsync(IGuildUser user){
            var document = new GuildUserModel{UserId = user.Id,
                                              Username = user.Username,
                                              GuildId = user.GuildId,
                                              GuildName = user.Guild.Name,
            };

            await GuildUsers.InsertOneAsync(document);

            Console.WriteLine($"discordbot.users: A new document for {document.Username} has been added.");
        }

        public async Task<GuildUserModel?> GetGuildUserAsync(IGuildUser user){
            var document = await GuildUsers.Find(_ => _.UserId == user.Id && _.GuildId == user.GuildId).FirstOrDefaultAsync();
            return document;
        }

        //public async Task UpdateGuildUserAsync(IGuildUser user, int? level = null, int? experience = null, int? currency = null){}

        public async Task DeleteGuildUserAsync(IGuildUser user){
            var userId = user.Id;
            var guildId = user.GuildId;
            var filter = Builders<GuildUserModel>.Filter.And(
                Builders<GuildUserModel>.Filter.Eq(_ => _.UserId, userId),
                Builders<GuildUserModel>.Filter.Eq(_ => _.GuildId, guildId)
            );
            
            await GuildUsers.DeleteOneAsync(filter);
        }

        // Warning Ops
        public async Task CreateWarningAsync(IGuildUser user, string? reason = null, double duration = 0){
            var document = new WarningModel{UserId = user.Id,
                                            Username = user.Username,
                                            GuildId = user.GuildId,
                                            GuildName = user.Guild.Name,
                                            Reason = reason ?? "No reason given",
                                            Created = new BsonDateTime(DateTime.UtcNow),
                                            Expires = duration == 0 ? null : new BsonDateTime(DateTime.UtcNow.AddDays(duration))
            };

            await Warnings.InsertOneAsync(document);
            Console.WriteLine($"discordbot.warnings: A new document for {document.Username} has been added.");
        }

        public async Task<List<WarningModel>?> GetUserWarningsAsync(IGuildUser user){
            var warnings = await Warnings.Find(_ => _.UserId == user.Id && _.GuildId == user.GuildId).ToListAsync();
            return warnings;
        }

        public async Task<List<WarningModel>?> GetGuildWarningsAsync(IGuild guild){
            var warnings = await Warnings.Find(_ => _.GuildId == guild.Id).ToListAsync();
            return warnings;
        }

        //public async Task UpdateWarningAsync(IGuildUser user, string? reason = null, int? duration = null){}

        public async Task DeleteWarningAsync(string id){
            var filter = Builders<WarningModel>.Filter.Eq(_ => _.Id, id);
            await Warnings.DeleteOneAsync(filter);
        }

        public async Task ClearWarningsAsync(IGuildUser user){
            var warnings = await GetUserWarningsAsync(user);
            if (warnings == null){
                return;
            }

            foreach(var warning in warnings){
                if (warning.Id == null)
                    continue;
                await DeleteWarningAsync(warning.Id);
            }

            Console.WriteLine($"discordbot.warnings: Warnings cleared for {user.Username} in {user.Guild.Name}.");
        }
    }
}