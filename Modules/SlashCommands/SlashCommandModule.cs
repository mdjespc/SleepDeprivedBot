using Discord.Interactions;
using Microsoft.Extensions.Logging;


namespace DiscordBot.Modules.SlashCommands;

//A slash command module must be public and inherit InteractionModuleBase in order to be discovered by AddModulesAsync
public class SlashCommandModule : InteractionModuleBase<SocketInteractionContext>{
    protected readonly ILogger _logger;

    public SlashCommandModule(ILogger<Bot> logger){
        _logger = logger;
    }

}