using Discord.Commands;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Modules.TextCommands;

//A text command module must be public and inherit ModuleBase in order to be discovered by AddModulesAsync.
public class TextCommandModule : ModuleBase<SocketCommandContext>{

    protected readonly IMongoDbService _db;
    protected readonly ILanguageManager _langManager;
    protected readonly ILogger _logger;

    public TextCommandModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger)
    {
        _db = db;
        _langManager = langManager;
        _logger = logger;
    }
}

