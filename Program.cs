/*
Github: https://github.com/mdjespc/DiscordBot
Disccord.net Github reference: https://github.com/discord-net/Discord.Net/tree/dev
Discord.net documentation reference: https://discordnet.dev/guides/introduction/intro.html
*/

using System.Reflection;
using Discord.Rest;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot
{
    internal class Program
    {
        static void Main(string[] args)
            => new Program()
            .MainAsync()
            .GetAwaiter()
            .GetResult();

        public async Task MainAsync()
        {
            /*
            Create a Configuration object that includes user secrets.
            The 'AddUserSecrets' method ensures that the user secrets from the executing assembly are part of the configuration.
            By calling 'AddUserSecrets(Assembly.GetExecutingAssembly())', the ConfigurationBuilder looks for user secrets associated with the specified assembly.
            */
            var config = new ConfigurationBuilder()
                            .AddUserSecrets(Assembly.GetExecutingAssembly())
                            .Build();
            /*
            Create a ServiceProvider object that references all project dependencies such that no direct dependencies are needed (will allow for easier unit-testing).
            In this case, it serves as a DI container for bot configuration and bot class.
            More on Dependency Injection: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
            More on Inversion of Dependency: https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles#dependency-inversion
            More on the 'ServiceProvider' class: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceprovider?view=dotnet-plat-ext-8.0
            */
            var services = new ServiceCollection()
                            .AddLogging(options =>{
                                options.ClearProviders();
                                options.AddConsole();
                            })
                            .AddSingleton<IConfiguration>(config)
                            .AddSingleton<ILogger<Bot>, Logger<Bot>>()
                            .AddSingleton<IMongoDbService, MongoDbService>()
                            .AddSingleton<ILanguageManager, LanguageManager>()
                            .AddScoped<IBot, Bot>()
                            .BuildServiceProvider();

            try
            {
                //Resolve MongoDB database and Language managing services as instances to inject
                IMongoDbService _mongoDbService = services.GetRequiredService<IMongoDbService>();
                ILanguageManager _langManager = services.GetRequiredService<ILanguageManager>();
                /*
                Resolve an instance of IBot from the DI container.
                The DI container automatically injects the required dependencies (ILogger<Bot> and IConfiguration) into the Bot class when creating the instance.
                */
                IBot _discordBot = services.GetRequiredService<IBot>();
                await _discordBot.StartAsync(services);

                //Close client and exit application by pressing the 'Q' key
                do
                {
                    var keyInput = Console.ReadKey();
                    
                    if(keyInput.Key == ConsoleKey.Q)
                    {
                        await _discordBot.StopAsync();
                        return;
                    }
                } while(true);

            }catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
                Environment.Exit(-1);
            }

        }

    }
}