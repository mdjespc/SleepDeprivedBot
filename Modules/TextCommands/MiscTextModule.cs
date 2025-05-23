using Discord.Commands;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;


namespace DiscordBot.Modules.TextCommands;


    
public class MiscTextModule : TextCommandModule
{
    public MiscTextModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger)
    {
    }
    
    [Command("echo")]
    [Summary("Echoes a message.")]
    public async Task EchoCommandAsync([Remainder][Summary("A phrase.")] string phrase)
    {
        await ReplyAsync(phrase);
    }

    [Command("help")]
    [Summary("Lists all commands.")]
    public async Task HelpCommandAsync()
    {
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

    [Command("kalek")]
    [Summary("Praises Kalek (the owner).")]
    public async Task KalekCommandAsync(){
        string replyMessage = "If Kalek has a million fans, I am one of them. If Kalek has 5 fans, I am one of them. If Kalek has one fan, that one is me." +
        " If Kalek has no fans, I am no longer alive. If the world is against Kalek, I am against the world. Till my last breath, I'll love Kalek.";
        await ReplyAsync(replyMessage);
    } 

}

