using Discord.Interactions;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;


namespace DiscordBot.Modules.SlashCommands;

//A slash command module must be public and inherit InteractionModuleBase in order to be discovered by AddModulesAsync
public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>{
    protected readonly IMongoDbService _db;
    protected readonly ILanguageManager _langManager;
    protected readonly ILogger _logger;

    public SlashCommandModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger){
        _db = db;
        _langManager = langManager;
        _logger = logger;
    }

    public enum EmbedColor
    {
        Default,
        Blue,
        Red,
        Green,
        Orange,
        Gold
    }

}