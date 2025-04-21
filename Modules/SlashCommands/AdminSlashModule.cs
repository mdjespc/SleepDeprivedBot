using Discord;
using Discord.Interactions;
using DiscordBot.Attributes;
using DiscordBot.Services;
using InteractionFramework.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot.Modules.SlashCommands;

[RequireAdminOrOwner]
//[DefaultMemberPermissions(GuildPermission.KickMembers)]
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

    //Moderation tools
    [SlashCommand("warn", "Send a warning to a member.")]
    public async Task WarnCommandAsync(IUser member, string reason = "",
    [Summary(description:"Channel to send the warning to. Leaving this blank will only send the warning to the member's DMs.")]ITextChannel? channel = null){
        reason = string.IsNullOrWhiteSpace(reason) ? "No reason given." : reason;

        var thumbnailUrl = member.GetAvatarUrl();
        string title = "Warning";
        string description = $"You have received a warning from the {Context.Guild.Name} moderation team.\n\n**Reason:**\n{reason}\n\n\nYou now have N/A active warnings.";
        var color = Discord.Color.Orange;

        EmbedBuilder embedBuilder = new EmbedBuilder(){
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color,
        };

        var warning = embedBuilder.Build();

        if (channel != null)
            await channel.SendMessageAsync(member.Mention, embed:warning);

        await member.SendMessageAsync("", embed:warning);
        await RespondAsync("User has been warned. You can view all active warnings by using /warnings.", ephemeral:true);

        //Log the warning action in a modlog channel, if one is set up.
        var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
        if (string.IsNullOrWhiteSpace(modlogChannelId))
            return;
        //ChannelID is passed as a string so we need to convert it to ulong type
        var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
        if (modlogChannel == null)
            return;

        title = "Moderation Action";
        description = $"**Action Type:** Warning\n\n**Issued by:** {Context.User}#{Context.User.Discriminator}\n\n**Issued to**: {member}#{member.Discriminator}\n\n**Reason:**\n{reason}";
        color = Discord.Color.Default;
        var modlog = new EmbedBuilder(){
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color,
        }.WithCurrentTimestamp()
         .Build();

        await modlogChannel.SendMessageAsync("", embed: modlog);
    }

    [SlashCommand("warnings", "View a list of all active warnings.")]
    public async Task WarningsCommandAsync(){
        await RespondAsync("Coming soon!");
    }


    [SlashCommand("mute", "Mute a member")]
    public async Task MuteCommandAsync(IUser member, [Summary(description:"Duration in minutes. Defaults to zero (indefinitely).")]int duration = 30, string reason = ""){
        member = member as IGuildUser ?? throw new Exception("Invalid User");
        
        await RespondAsync("Coming soon!");
    }
}