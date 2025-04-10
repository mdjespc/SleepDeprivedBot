using Microsoft.Extensions.Configuration;

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

        public async Task SetGuildSettingsAsync(ulong guildId, string key, string value){
            var filter = Builders<GuildSettingsModel>.Filter.Eq(_ => _.GuildId, guildId);

            //Create the update based on the setting key.
            var update = key switch
            {
                "language" => Builders<GuildSettingsModel>.Update.Set(_ => _.Language, value),
                "prefix" => Builders<GuildSettingsModel>.Update.Set(_ => _.Prefix, value),
                "welcomeChannel" => Builders<GuildSettingsModel>.Update.Set(_ => _.welcomeChannel, value),
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

                Console.WriteLine($"`discordbot.guilds`: Guild Settings for Guild {guildId} were not found in the database. A new document for this guild has been added.");
            }

            return settings;
        }
    }
}