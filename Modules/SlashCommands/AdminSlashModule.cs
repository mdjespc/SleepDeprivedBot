using Discord;
using Discord.Interactions;
using DiscordBot.Attributes;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
//using DiscordBot.Attributes;
using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Modules.SlashCommands;

[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers)]
public class AdminSlashModule : SlashCommandModule{
    public AdminSlashModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger) {}

    
    [SlashCommand("announce", "Sends an announcement to the target channel.")]
    public async Task AnnounceCommandAsync(INewsChannel targetChannel, string announcement){
        if (string.IsNullOrWhiteSpace(announcement)){
            await RespondAsync("Announcement cannot be empty.");
            return;
        }
        await targetChannel.SendMessageAsync(announcement);
    }

    [SlashCommand("embed", "Sends an embedded message to the target channel")]
    public async Task EmbedCommandAsync(IChannel targetChannel, string title, string description, EmbedColor color = EmbedColor.Default){
        ITextChannel channel = targetChannel as ITextChannel ?? throw new ArgumentException("<channel> must be a text channel.");
        Discord.Color embedColor;
        switch (color){
            case EmbedColor.Blue: 
                embedColor = Discord.Color.Blue;
                break;
            case EmbedColor.Red: 
                embedColor = Discord.Color.Blue;
                break;
            case EmbedColor.Green: 
                embedColor = Discord.Color.Green;
                break;
            case EmbedColor.Orange: 
                embedColor = Discord.Color.Orange;
                break;
            case EmbedColor.Gold: 
                embedColor = Discord.Color.Gold;
                break;
            default:
                embedColor = Discord.Color.Default;
                break;
        }
        
        EmbedBuilder embedBuilder = new EmbedBuilder(){
            Title = title,
            Description = description,
            Color = embedColor
        };

        await channel.SendMessageAsync("", false, embedBuilder.Build());
        await RespondAsync("Embed created! :white_check_mark:", ephemeral:true);
    }


    [SlashCommand("message", "Sends a text message to the target channel.")]
    public async Task MessageCommandAsync(IChannel targetChannel, string message){
        ITextChannel textChannel;

        try{
            textChannel = targetChannel as ITextChannel ?? throw new ArgumentException("<channel> must be a text channel.");
            message = string.IsNullOrWhiteSpace(message) ? throw new ArgumentException("<message> required.") : message; 
        }catch(ArgumentException exception){
            _logger.LogError(exception, "Error on command execute: ");
            await RespondAsync($"{exception.Message}\n\nUsage: /send message <channel> <message>");
            return;
        }
        
        await textChannel.SendMessageAsync(message);
        await RespondAsync("Message sent! :white_check_mark:", ephemeral:true);
    }

    [SlashCommand("setup", "Start configuring the bot settings")]
    public async Task SetupCommandAsync(){
        string desc = @"Please select a language for this server.
        
        Por favor seleccionar un idioma para este servidor.
        
        Veuillez choisir une langue pour ce serveur.";

        var embed = new EmbedBuilder()
            .WithTitle("SleepDeprivedBot Setup")
            .WithDescription(desc)
            .WithColor(Discord.Color.Blue)
            .Build();

        var buttons = new ComponentBuilder()
            .WithButton("English", "setup_lang_en", ButtonStyle.Primary, new Emoji("🇬🇧"))
            .WithButton("Español", "setup_lang_es", ButtonStyle.Primary, new Emoji("🇪🇸"))
            .WithButton("Français", "setup_lang_fr", ButtonStyle.Primary, new Emoji("🇫🇷"))
            .WithButton("Finish Setup", "setup_finish", ButtonStyle.Success, new Emoji("✅"), row: 1);;

        await RespondAsync(embed: embed, components: buttons.Build());
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_lang_en")]
    public async Task SetupLangEnAsync(){
        await DeferAsync(); //Acknowledge the interaction
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "language", "en");
        await FollowupAsync("Your preferred language has been set to English✅", ephemeral: true);
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_lang_es")]
    public async Task SetupLangEsAsync(){
        await DeferAsync();
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "language", "es");
        await FollowupAsync("Su idioma preferido se ha establecido en Español✅", ephemeral: true);
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_lang_fr")]
    public async Task SetupLangFrAsync(){
        await DeferAsync();
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "language", "fr");
        await FollowupAsync("Votre langue préférée a été définie sur le Français✅", ephemeral: true);
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_finish")]
    public async Task FinishSetupAsync()
    {
        await DeferAsync();
        await FollowupAsync(_langManager.GetString("setup_followup", _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Language), ephemeral: false);
    }


}