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
[Group("set", "Bot configuration commands")]
public class SetupSlashModule : SlashCommandModule{
    public SetupSlashModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger) {}

    [SlashCommand("language", "Set SleepDeprivedBot's Language")]
    public async Task SetupCommandAsync(){
        string desc = @"Please select a language for this server.
        
Por favor seleccionar un idioma para este servidor.
        
Veuillez choisir une langue pour ce serveur.";

        var embed = new EmbedBuilder()
            .WithTitle("SleepDeprivedBot Setup")
            .WithDescription(desc)
            .WithColor(Discord.Color.Blue);
            //.Build();

        var buttons = new ComponentBuilder()
            .WithButton("English", "setup_lang_en", ButtonStyle.Primary, new Emoji("🇬🇧"))
            .WithButton("Español", "setup_lang_es", ButtonStyle.Primary, new Emoji("🇪🇸"))
            .WithButton("Français", "setup_lang_fr", ButtonStyle.Primary, new Emoji("🇫🇷"))
            .WithButton("Finish Setup", "setup_finish", ButtonStyle.Success, new Emoji("✅"), row: 1);;

        await RespondAsync(embed: embed.Build(), components: buttons.Build());
    }

    [Group("welcome", "Set new member welcome messages")]
    public class WelcomeSubGroup : SlashCommandModule{
        public WelcomeSubGroup(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger) {}

        [SlashCommand("channel", "Set up a welcome channel for new members.")]
        public async Task WelcomeChannelCommandAsync(ITextChannel channel){
            await _db.SetGuildSettingsAsync(Context.Guild.Id, "welcomeChannel", channel.Id.ToString());
            await RespondAsync("Welcome channel set!", ephemeral:true);
        }

        [SlashCommand("off", "Disable welcome messages.")]
        public async Task WelcomeOffCommandAsync(){
            await _db.SetGuildSettingsAsync(Context.Guild.Id, "welcomeMessage", "");
            await RespondAsync("Welcome channel disabled.", ephemeral:true);
        }

        [SlashCommand("message", "Set up a welcome message for new members.")]
        public async Task WelcomeMessageCommandAsync(string message){
            //TODO '@u' literals will be replaced with the user name
            await _db.SetGuildSettingsAsync(Context.Guild.Id, "welcomeMessage", message);
            await RespondAsync("Welcome message set!", ephemeral:true);
        }
    }

    [SlashCommand("modlog", "Set up a moderation log channel.")]
    public async Task ModlogCommandAsync(ITextChannel channel){
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "modlog", channel.Id.ToString());
        await RespondAsync("Modlog enabled.", ephemeral:true);
    }

    //Setup Module Interaction Components
    
    //[DoUserCheck]
    [ComponentInteraction("setup_lang_en")]
    public async Task SetupLangEnAsync(){
        Console.WriteLine("Lang EN interaction triggered");
        await DeferAsync(); //Acknowledge the interaction
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "language", "en");
        await FollowupAsync("Your preferred language has been set to English✅", ephemeral: true);
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_lang_es")]
    public async Task SetupLangEsAsync(){
        Console.WriteLine("Lang ES interaction triggered");
        await DeferAsync();
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "language", "es");
        await FollowupAsync("Su idioma preferido se ha establecido en Español✅", ephemeral: true);
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_lang_fr")]
    public async Task SetupLangFrAsync(){
        Console.WriteLine("Lang FR interaction triggered");
        await DeferAsync();
        await _db.SetGuildSettingsAsync(Context.Guild.Id, "language", "fr");
        await FollowupAsync("Votre langue préférée a été définie sur le Français✅", ephemeral: true);
    }

    //[DoUserCheck]
    [ComponentInteraction("setup_finish")]
    public async Task FinishSetupAsync()
    {
        Console.WriteLine("FINISH interaction triggered");
        await DeferAsync();
        await FollowupAsync(_langManager.GetString("setup_followup", _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Language), ephemeral: false);
    }



}