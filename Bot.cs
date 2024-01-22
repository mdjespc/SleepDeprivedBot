using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.ComponentModel;
using System.Threading.Channels;


namespace DiscordBot{
    public class Bot : IBot{
        private ServiceProvider? _serviceProvider;
        
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        /*
        The ILogger<Bot> and IConfiguration objects are provided by the DI container when the Bot class is instantiated through dependency injection.
        The DI container takes care of resolving and injecting these dependencies into the Bot class constructor.
        */
        public Bot(ILogger<Bot> logger,
                IConfiguration config)
        {
            _config = config;
            _logger = logger;

            //Initialize a DiscordSocketClient object that maintains communication between the bot and a server with the specified configuration.
            DiscordSocketConfig clientConfiguration = new (){
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(clientConfiguration);
            // Retrieve CommandService instance via ctor
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services){
            //Retrieve API token from User Secrets
            string apiToken = _config["DISCORD_BOT_TOKEN"] ?? throw new Exception("Missing Discord Bot Token");
            _serviceProvider = services;

            _logger.LogInformation($"Initializing with token {apiToken}");

            await _client.LoginAsync(TokenType.Bot, apiToken);
            await _client.StartAsync();
            _logger.LogInformation("Connected");

            //Discover all of the command modules in the entry assembly and load them.     
            await _commands.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), 
                                            services: _serviceProvider);
            
            // Hook the MessageReceived event into the command handler
            _client.MessageReceived += HandleCommandAsync;
            _logger.LogInformation("Command modules loaded.");
        }
        
        public async Task StopAsync(){
            _logger.LogInformation("Disconnecting...");
            await _client.StopAsync();
        }

        
        
        private async Task HandleCommandAsync(SocketMessage messageParam){
            //Ignore system messages and messages from bots
            var message = messageParam as SocketUserMessage;
            if (message == null || message.Author.IsBot) return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Log the received message
            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - {message.Author} in {context.Guild.Name}: {message.Content}");

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            int argPos = 0;
            char commandPrefix = '!';
            bool messageIsCommand = message.HasCharPrefix(commandPrefix, ref argPos);
            if (!messageIsCommand) return;

            // Execute the command with the command context we created if it exists in the ServiceCollection.
            await _commands.ExecuteAsync(
                context: context, 
                argPos: argPos,
                services: _serviceProvider);
        }
        
    }


}