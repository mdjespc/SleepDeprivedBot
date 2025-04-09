using Discord.Interactions;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;


namespace DiscordBot.Modules.SlashCommands;

//A slash command module must be public and inherit InteractionModuleBase in order to be discovered by AddModulesAsync
public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>{
    protected readonly IMongoDbService _db;
    protected readonly ILogger _logger;

    public SlashCommandModule(IMongoDbService db, ILogger<Bot> logger){
        _db = db;
        _logger = logger;
    }

}