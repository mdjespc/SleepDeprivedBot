using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Modules.TextCommands;

//A text command module must be public and inherit ModuleBase in order to be discovered by AddModulesAsync.
public class TextCommandModule : ModuleBase<SocketCommandContext>{

    protected readonly ILogger _logger;

    public TextCommandModule(ILogger<Bot> logger)
    {
        _logger = logger;
    }
}

