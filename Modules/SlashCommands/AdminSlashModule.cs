using Discord;
using Discord.Interactions;
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
}