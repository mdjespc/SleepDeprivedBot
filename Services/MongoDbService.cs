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


    }
}