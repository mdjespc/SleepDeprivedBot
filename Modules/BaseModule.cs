using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Modules
{
    public class BaseModule : ModuleBase<SocketCommandContext>{

        protected readonly ILogger _logger;

        public BaseModule(ILogger<Bot> logger)
        {
            _logger = logger;
        }
    }

}