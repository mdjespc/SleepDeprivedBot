using Discord;
using Discord.Interactions;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
//using DiscordBot.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules.SlashCommands;

public class MiscSlashModule : SlashCommandModule{
    public MiscSlashModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger){}


    [SlashCommand("echo", "Repeat the input")]
    public async Task EchoCommandAsync(string input, [Summary(description: "Mention the user")] bool mention = false)
        => await RespondAsync(string.Concat(input, " ", mention ? Context.User.Mention : string.Empty));
    
    [SlashCommand("ping", "Pings the bot and returns its latency")]
    public async Task PingCommandAsync([Summary(description: "Visible only to you")] bool ephemeral = false)
        => await RespondAsync(text: _langManager.GetString("ping_command", _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Language, Context.Client.Latency), ephemeral: ephemeral);

    [SlashCommand("bitrate", "Returns the bitrate of a voice channel")]
    public async Task BitrateCommandAsync([ChannelTypes(ChannelType.Voice, ChannelType.Stage)] IVoiceChannel channel, [Summary(description: "Visible only to you")] bool ephemeral = false)
    {
        channel = channel ?? throw new ArgumentNullException("<channel> must be a voice channel.");
        //await RespondAsync(text: $"{channel.Name} has a bitrate of {channel.Bitrate} bits per second.", ephemeral: ephemeral);
        await RespondAsync(text: _langManager.GetString("bitrate_command", _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Language, channel.Name, channel.Bitrate), ephemeral: ephemeral);
    }

    [SlashCommand("help", "Lists all commands")]
    public async Task HelpCommandAsync(){
        string file = "Modules\\help.txt";
        string? helpMessage = null;
        try{
            helpMessage = File.ReadAllText(file);
        }catch(FileNotFoundException exception)
        {
            _logger.LogError(exception, $"File not found: {exception.FileName}");
        }
        helpMessage ??= "Unable to retrieve help list.";
        await ReplyAsync(helpMessage);
    }


    [SlashCommand("kalek", "Praises Kalek (the owner)")]
    public async Task KalekCommandAsync(){
        string replyMessage = "If Kalek has a million fans, I am one of them. If Kalek has 5 fans, I am one of them. If Kalek has one fan, that one is me." +
        " If Kalek has no fans, I am no longer alive. If the world is against Kalek, I am against the world. Till my last breath, I'll love Kalek.";
        await RespondAsync(replyMessage);
    }
}