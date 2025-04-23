using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
[DefaultMemberPermissions(GuildPermission.ModerateMembers)]
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
    public async Task MuteCommandAsync(IGuildUser member, [Summary(description:"Duration in minutes. Defaults to zero (indefinitely).")]int duration = 0, string reason = ""){
        //Create a "Muted" role if it does not yet exist
        string roleName = "Muted";
        ulong roleId;
        var role = Context.Guild.Roles.FirstOrDefault(_ => _.Name.Equals(roleName));

        if (role != null)
            roleId = role.Id;
        else{
            var permissions = new GuildPermissions(
                                            createInstantInvite: false,
                                            kickMembers: false,
                                            banMembers: false,
                                            administrator: false,
                                            manageChannels: false,
                                            manageGuild: false,
                                            addReactions: false,
                                            viewAuditLog: false,
                                            viewGuildInsights: false,
                                            viewChannel: true,
                                            sendMessages: false,
                                            manageMessages: false,
                                            embedLinks: false,
                                            attachFiles: false,
                                            readMessageHistory: true,
                                            mentionEveryone: false,
                                            useExternalEmojis: false,
                                            connect: false,
                                            speak: false,
                                            muteMembers: false,
                                            deafenMembers: false,
                                            moveMembers: false,
                                            useVoiceActivation: false,
                                            prioritySpeaker: false,
                                            stream: false,
                                            changeNickname: false,
                                            manageNicknames: false,
                                            manageRoles: false,
                                            manageWebhooks: false,
                                            manageEmojisAndStickers: false,
                                            useApplicationCommands: false,
                                            requestToSpeak: false,
                                            manageEvents: false,
                                            manageThreads: false,
                                            createPublicThreads: false,
                                            createPrivateThreads: false,
                                            useExternalStickers: false,
                                            sendMessagesInThreads: false,
                                            startEmbeddedActivities: false,
                                            moderateMembers: false,
                                            useSoundboard: false,
                                            viewMonetizationAnalytics: false,
                                            sendVoiceMessages: false,
                                            useClydeAI: false,
                                            createGuildExpressions: false,
                                            setVoiceChannelStatus: false
            );
            var newRole = await Context.Guild.CreateRoleAsync(roleName, permissions, color: Discord.Color.Default);
            roleId = newRole.Id;
        }
        
        //Establish the role to add
        var mutedRole = Context.Guild.GetRole(roleId);

        if (member.RoleIds.Contains(roleId)){
            await RespondAsync("Member is already muted. If you believe this to be a mistake, check that the \"Muted\" role has the \"Send Messages\" permission turned off.", ephemeral: true);
            return;
        }

        //Add role and let the user know
        await member.AddRoleAsync(mutedRole);
        await RespondAsync("Member has been muted.", ephemeral: true);

        reason = string.IsNullOrWhiteSpace(reason) ? "No reason given." : reason;
        var thumbnailUrl = member.GetAvatarUrl();
        string title = "Mute Notice";
        string description = $"You have been muted by the {Context.Guild.Name} moderation team.\n\n**Reason:**\n{reason}\n\n**Duration:** {(duration == 0 ? "Unspecified" : $"{duration} minutes")}";
        var color = Discord.Color.Red;

        var notice = new EmbedBuilder(){
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color,
        }.WithCurrentTimestamp()
        .Build();

        await member.SendMessageAsync("", embed: notice);

        //Time the unmute if duration has been specified
        if (duration > 0){
            _ = Task.Run(async () => {
                await Task.Delay(TimeSpan.FromMinutes(duration));
                await member.RemoveRoleAsync(mutedRole);
            });
        }

        //Log the mod action in a modlog channel, if one is set up.
        var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
        if (string.IsNullOrWhiteSpace(modlogChannelId))
            return;
        //ChannelID is passed as a string so we need to convert it to ulong type
        var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
        if (modlogChannel == null)
            return;

        var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
        title = "Moderation Action";
        description = $"**Action Type:** Mute\n\n**Issued by:** {Context.User.Username}\n\n**Issued to**: {member.Username}\n\n**Reason:**\n{reason}\n\n**Duration:** {(duration == 0 ? "Unspecified" : $"{duration} minutes")}";
        color = Discord.Color.Red;
        var modlog = new EmbedBuilder(){
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color,
        }.WithCurrentTimestamp()
         .Build();

        await modlogChannel.SendMessageAsync("", embed: modlog);
    }

    [SlashCommand("unmute", "Unmute a member")]
    public async Task UnmuteCommandAsync(IGuildUser member){
        string roleName = "Muted";
        var role = Context.Guild.Roles.FirstOrDefault(_ => _.Name.Equals(roleName));

        if (role == null || !member.RoleIds.Contains(role.Id)){
            await RespondAsync("This user is not muted, or has not been muted by this bot.");
            return;
        }

        await member.RemoveRoleAsync(role);
        await RespondAsync("Member has been unmuted.", ephemeral: true);

        string title = "Unmute Notice";
        string description = $"You have been unmuted by the {Context.Guild.Name} moderation team.";
        var color = Discord.Color.Default;
        var notice = new EmbedBuilder(){
            Title = title,
            Description = description,
            Color = color,
        }.WithCurrentTimestamp()
        .Build();
        await member.SendMessageAsync("", embed: notice);

        //Log the mod action in a modlog channel, if one is set up.
        var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
        if (string.IsNullOrWhiteSpace(modlogChannelId))
            return;
        //ChannelID is passed as a string so we need to convert it to ulong type
        var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
        if (modlogChannel == null)
            return;

        var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
        var thumbnailUrl = member.GetAvatarUrl();
        title = "Moderation Action";
        description = $"**Action Type:** Unmute\n\n**Issued by:** {Context.User.Username}\n\n**Issued to**: {member.Username}";
        color = Discord.Color.Red;
        var modlog = new EmbedBuilder(){
            Author = author,
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color,
        }.WithCurrentTimestamp()
         .Build();

        await modlogChannel.SendMessageAsync("", embed: modlog);
    }

    [SlashCommand("kick", "Kick a member.")]
    public async Task KickCommandAsync(IGuildUser member, string reason = ""){
        var user = (IUser) member;
        reason = string.IsNullOrWhiteSpace(reason) ? "No reason provided." : reason;

        //Prioritize the mod action, then send notice to ex-member
        await member.KickAsync(reason: reason);

        var title = "Kick Notice";
        var description = $"You have been kicked from {Context.Guild.Name}.\n\n**Reason:**\n{reason}";
        var color = Discord.Color.Red;
        
        var notice = new EmbedBuilder(){
            Title = title,
            Description = description,
            Color = color
        }.WithCurrentTimestamp()
        .Build();
        await user.SendMessageAsync("", embed: notice);
        
        //THEN log the mod action
        await RespondAsync("Member has been kicked from the server.", ephemeral: true);

        //Log the mod action in a modlog channel, if one is set up.
        var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
        if (string.IsNullOrWhiteSpace(modlogChannelId))
            return;
        //ChannelID is passed as a string so we need to convert it to ulong type
        var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
        if (modlogChannel == null)
            return;

        var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
        var thumbnailUrl = user.GetAvatarUrl();
        title = "Moderation Action";
        description = $"**Action Type:** Kick\n\n**Issued by:** {Context.User.Username}\n\n**Issued to**: {user.Username}\n\n**Reason:**\n{reason}";

        var modlog = new EmbedBuilder(){
            Author = author,
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color
        }.WithCurrentTimestamp()
        .Build();

        await modlogChannel.SendMessageAsync("", embed: modlog);
    }

    [SlashCommand("ban", "Ban a member.")]
    public async Task BanCommandAsync(IGuildUser member, string reason = ""){
        var user = (IUser) member;
        reason = string.IsNullOrWhiteSpace(reason) ? "No reason provided." : reason;

        //Prioritize the mod action, then send notice to ex-member
        await member.BanAsync(reason: reason);

        var title = "Ban Notice";
        var description = $"You have been banned from {Context.Guild.Name}.\n\n**Reason:**\n{reason}";
        var color = Discord.Color.DarkRed;
        
        var notice = new EmbedBuilder(){
            Title = title,
            Description = description,
            Color = color
        }.WithCurrentTimestamp()
        .Build();
        await user.SendMessageAsync("", embed: notice);
        
        //then log the mod action
        await RespondAsync("Member has been banned from the server.", ephemeral: true);

        //Log the mod action in a modlog channel, if one is set up.
        var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
        if (string.IsNullOrWhiteSpace(modlogChannelId))
            return;
        //ChannelID is passed as a string so we need to convert it to ulong type
        var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
        if (modlogChannel == null)
            return;

        var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
        var thumbnailUrl = user.GetAvatarUrl();
        title = "Moderation Action";
        description = $"**Action Type:** Ban\n\n**Issued by:** {Context.User.Username}\n\n**Issued to**: {user.Username}\n\n**Reason:**\n{reason}";

        var modlog = new EmbedBuilder(){
            Author = author,
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color
        }.WithCurrentTimestamp()
        .Build();

        await modlogChannel.SendMessageAsync("", embed: modlog);
    }

    // [SlashCommand("unban", "Unban a member")]
    // public async Task UnbanCommandAsync(ulong id){

    // }
}