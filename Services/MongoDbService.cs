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

        //Check guild settings — or initialize them if non-existent 
        public async Task<GuildSettingsModel> GetGuildSettingsAsync(ulong guildId){
            var settings = await Guilds.Find(_ => _.GuildId == guildId).FirstOrDefaultAsync();

            if (settings == null){
                settings = new GuildSettingsModel{GuildId = guildId};
                await Guilds.InsertOneAsync(settings);

                Console.WriteLine($"Guild Settings for Guild {guildId} were not found in the database. A new document for this guild has been added.");
            }

            return settings;
        }
    }
}