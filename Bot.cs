using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Discord;
using Discord.Interactions;
using Discord.Commands;
using Discord.WebSocket;
using System.ComponentModel;
using System.Threading.Channels;
using DiscordBot.Services;


namespace DiscordBot{
    public class Bot : IBot{
        private ServiceProvider? _serviceProvider;
        
        private readonly IConfiguration _config;
        private readonly IMongoDbService _db;
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly CommandService _commands;


        /*
        The ILogger<Bot> and IConfiguration objects are provided by the DI container when the Bot class is instantiated through dependency injection.
        The DI container takes care of resolving and injecting these dependencies into the Bot class constructor.
        */
        public Bot(IConfiguration config,
                IMongoDbService db,
                ILogger<Bot> logger)
        {
            _config = config;
            _db = db;
            _logger = logger;

            //Initialize a DiscordSocketClient object that maintains communication between the bot and a server with the specified configuration.
            DiscordSocketConfig clientConfiguration = new (){
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };
            _client = new DiscordSocketClient(clientConfiguration);


            //Initialize an interaction handler that will execute slash commmands
            var interactionServiceConfiguration = new InteractionServiceConfig();
            _interactionService = new InteractionService(_client, interactionServiceConfiguration);

            // Retrieve CommandService for text-based commands instance via ctor
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services){
            //Retrieve API token from User Secrets
            string apiToken = _config["DISCORD_BOT_TOKEN"] ?? throw new Exception("Missing Discord Bot Token");
            _serviceProvider = services;

            _logger.LogInformation($"Initializing with token {apiToken}");

            //Process when the client is ready to register slash commands
            _client.Ready += ReadyAsync;
            _interactionService.Log += LogAsync;

            
            await _client.LoginAsync(TokenType.Bot, apiToken);
            await _client.StartAsync();
            _logger.LogInformation("Connected");

            //Discover all of the slash command modules in the entry assembly and load them. 
            await _interactionService.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(),
                                                      services: _serviceProvider);

            //Discover all of the text command modules in the entry assembly and load them.     
            await _commands.AddModulesAsync(assembly: Assembly.GetExecutingAssembly(), 
                                            services: _serviceProvider);
            

            //Hook the InteractionCreated and InteractionExecuted event handlers for slash commands
            _client.InteractionCreated += OnInteractionCreatedAsync;
            _interactionService.InteractionExecuted += OnInteractionExecuted;
            _logger.LogInformation("Slash command modules loaded.");


            // Hook the MessageReceived event handler and the OnCommandExecuted event handler for text commands
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            _logger.LogInformation("Text command modules loaded.");
        }
        
        public async Task StopAsync(){
            _logger.LogInformation("Disconnecting...");
            await _client.StopAsync();
        }

        //Registers slash commands globally.
        private async Task ReadyAsync(){
            await _interactionService.RegisterCommandsGloballyAsync();
        }

        private Task LogAsync(LogMessage log){
            _logger.LogInformation(log.ToString());
            return Task.CompletedTask;
        }
        
        private async Task OnInteractionCreatedAsync(SocketInteraction interaction){
            try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(_client, interaction);
            
            // Execute the incoming command.
            var result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            // Due to async nature of InteractionFramework, the result here may not always be success.
            // That's why we also need to handle the InteractionExecuted event.
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        //TODO
                        _logger.LogError($"Command Execution Error: {interaction} was executed at {DateTime.Now.ToShortTimeString()} by {context.User.Username} in {context.Guild.Name} in #{context.Channel.Name}\nError:{result.Error}");
                        break;
                    default:
                        _logger.LogError($"Command Execution Error: {interaction} was executed at {DateTime.Now.ToShortTimeString()} by {context.User.Username} in {context.Guild.Name} in #{context.Channel.Name}\nError:{result.Error}");
                        break;
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
        }

        private Task OnInteractionExecuted(ICommandInfo commandInfo, IInteractionContext context, Discord.Interactions.IResult result){
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        _logger.LogError($"Command Execution Error: {commandInfo.Name} was executed at {DateTime.Now.ToShortTimeString()} by {context.User.Username} in {context.Guild.Name} #{context.Channel.Name}\nError:{result.Error}");
                        break;
                    default:
                        break;
                }

            _logger.LogInformation($"Command Execution: {commandInfo.Name} was executed at {DateTime.Now.ToShortTimeString()} by {context.User.Username}#{context.User.Discriminator} in {context.Guild.Name} in #{context.Channel.Name}");
            return Task.CompletedTask;
    }
        
        //Logs messages and executes the command if the message is a text-based command.
        private async Task HandleCommandAsync(SocketMessage messageParam){
            //Ignore system messages and messages from bots
            var message = messageParam as SocketUserMessage;
            if (message == null || message.Author.IsBot) return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Log the received message
            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - {message.Author} in {context.Guild.Name} #{context.Channel.Name}: {message.Content}");

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            int argPos = 0;
            char commandPrefix = '!';
            bool messageIsCommand = message.HasCharPrefix(commandPrefix, ref argPos);

            if (!messageIsCommand)
                return;

            await _commands.ExecuteAsync(
                context: context, 
                argPos: argPos,
                services: _serviceProvider);
        }

        #pragma warning disable CS1998
        private async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result){
            var commandName = command.IsSpecified ? command.Value.Name : "A command";

            //Reply with an error message from the execution attempt if there is one
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                _logger.LogError(
                    $"Command Execution Error: {commandName} was executed at {DateTime.Now.ToShortTimeString()} by {context.User.Username} in {context.Guild.Name} #{context.Channel.Name}" +
                    $"\n\tError Reason: {result.ErrorReason}");
                //TODO Uncomment the line below after customizable prefixes have been implemented
                //await context.Channel.SendMessageAsync(result.ErrorReason + " Type !help to view a list of commands with their respective usages.");
            }

            _logger.LogInformation($"Command Execution: {commandName} was executed at {DateTime.Now.ToShortTimeString()} by {context.User.Username} in {context.Guild.Name} #{context.Channel.Name}");
            
        }

    }
}