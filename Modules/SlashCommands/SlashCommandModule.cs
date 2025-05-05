using System.Threading.Tasks;
using Discord;
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

    public IMessageChannel? GetGuildModlogChannelAsync(IGuild guild){
        var settings = _db.GetGuildSettingsAsync(guild.Id).Result;
        if (string.IsNullOrWhiteSpace(settings.Modlog))
            return null;
        //ChannelID is stored in db as a string so we need to convert it to ulong type
        return guild.GetChannelAsync(ulong.Parse(settings.Modlog)) as IMessageChannel;
    }

    public async Task SendModlogAsync(IMessageChannel channel,
                                      EmbedAuthorBuilder? author = null,
                                      string? thumbnailUrl = null,
                                      string? title = null,
                                      string? description = null,
                                      Color? color = null){
        
        var modlog = new EmbedBuilder(){
            Author = author,
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color
        }.WithCurrentTimestamp()
        .Build();

        await channel.SendMessageAsync("", embed: modlog);
    }

}