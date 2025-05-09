using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
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

[RequireStaff]
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

    //Role command group
    [Group("role", "Manage server roles.")]
    public class RoleSubGroup : SlashCommandModule{
        public RoleSubGroup(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger){}

        [SlashCommand("create", "Create a new role.")]
        public async Task RoleCreateCommandAsync(string name, string? color = "", [Summary(description:"Set to true if role should be displayed separately in the member list.")]bool hoist = false){
            IRole? role = null;
            try{
                var _color  = string.IsNullOrWhiteSpace(color) ? Discord.Color.Default : new Discord.Color(uint.Parse(color));
                role = await Context.Guild.CreateRoleAsync(name, color: _color, isHoisted: hoist);
            }catch(Exception e){
                _logger.LogError($"Exception happened while attempting to create a guild role: {e.Message}");
                await RespondAsync($"Could not create custom role.", ephemeral: true);
                return;
            }
            
            //_logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - Role \"{role.Name}\" created in {Context.Guild.Name}.");
            await RespondAsync($"Role {role.Mention} created!✅");


            var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
            if (string.IsNullOrWhiteSpace(modlogChannelId))
                return;
            var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
            if (modlogChannel == null)
                return;

            var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
            var title = "Role Created";
            var description = $"**Mention:** {role.Mention}\n**Hoisted**: {role.IsHoisted}\n**Created by:** {Context.User.Username}\n";
            var modlog = new EmbedBuilder(){
                Author = author,
                Title = title,
                Description = description,
                Color = Discord.Color.Green,
            }.WithCurrentTimestamp()
            .Build();

            await modlogChannel.SendMessageAsync("", embed: modlog);
        }

        [SlashCommand("delete", "Delete an existing role.")]
        public async Task RoleDeleteCommandAsync(IRole role){
            var name = role.Name;
            await role.DeleteAsync();
            
            //_logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - Role \"{role.Name}\" deleted in {Context.Guild.Name}.");
            await RespondAsync($"Role {name} deleted.");


            var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
            if (string.IsNullOrWhiteSpace(modlogChannelId))
                return;
            var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
            if (modlogChannel == null)
                return;

            var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
            var title = "Role Deleted";
            var description = $"**Name:** {name}\n**Hoisted**: {role.IsHoisted}\n**Deleted by:** {Context.User.Username}\n";
            var modlog = new EmbedBuilder(){
                Author = author,
                Title = title,
                Description = description,
                Color = Discord.Color.Red,
            }.WithCurrentTimestamp()
            .Build();

            await modlogChannel.SendMessageAsync("", embed: modlog);
        }

        [SlashCommand("assign", "Assign an existing role to a member.")]
        public async Task RoleAssignCommandAsync(IGuildUser user, IRole role, uint? duration = null){
            try{
                await user.AddRoleAsync(role);
            }catch(Exception e){
                _logger.LogError($"{DateTime.Now.ToShortTimeString()} - Role \"{role.Name}\" could not be assigned to {user.Username} in {Context.Guild.Name}: {e.Message}.");
                await RespondAsync($"Role {role.Mention} could not be assigned to {user.Mention}.", ephemeral: false);
            }

             _logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - Role \"{role.Name}\" assigned to {user.Username} in {Context.Guild.Name}.");
            await RespondAsync($"Role {role.Mention} assigned to {user.Mention}.");
            var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
            if (string.IsNullOrWhiteSpace(modlogChannelId))
                return;
            var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
            if (modlogChannel == null)
                return;

            var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
            var thumbnailUrl = user.GetAvatarUrl();
            var title = $"Role Assigned";
            var description = $"**Role:** {role.Mention}\n**To:** {user.Mention}\n**Assigned by:** {Context.User.Username}\n";
            var modlog = new EmbedBuilder(){
                Author = author,
                ThumbnailUrl = thumbnailUrl,
                Title = title,
                Description = description,
                Color = Discord.Color.Green,
            }.WithCurrentTimestamp()
            .Build();

            await modlogChannel.SendMessageAsync("", embed: modlog);
        }

        [SlashCommand("unassign", "Unassign a role from a member.")]
        public async Task RoleUnassignCommandAsync(IGuildUser user, IRole role){
            try{
                await user.RemoveRoleAsync(role);
            }catch(Exception e){
                _logger.LogError($"{DateTime.Now.ToShortTimeString()} - Role \"{role.Name}\" could not be unassigned from {user.Username} in {Context.Guild.Name}: {e.Message}.");
                await RespondAsync($"Role {role.Mention} could not be unassigned from {user.Mention}.", ephemeral: false);
            }

             _logger.LogInformation($"{DateTime.Now.ToShortTimeString()} - Role \"{role.Name}\" unassigned from {user.Username} in {Context.Guild.Name}.");
            await RespondAsync($"Role {role.Mention} unassigned from {user.Mention}.");
            var modlogChannelId = _db.GetGuildSettingsAsync(Context.Guild.Id).Result.Modlog;
            if (string.IsNullOrWhiteSpace(modlogChannelId))
                return;
            var modlogChannel = Context.Guild.GetChannel(ulong.Parse(modlogChannelId)) as IMessageChannel;
            if (modlogChannel == null)
                return;

            var author = new EmbedAuthorBuilder(){
                Name = Context.User.GlobalName,
                IconUrl = Context.User.GetAvatarUrl()
            };
            var thumbnailUrl = user.GetAvatarUrl();
            var title = $"Role Unassigned";
            var description = $"**Role:** {role.Mention}\n**From:** {user.Mention}\n**Unassigned by:** {Context.User.Username}\n";
            var modlog = new EmbedBuilder(){
                Author = author,
                ThumbnailUrl = thumbnailUrl,
                Title = title,
                Description = description,
                Color = Discord.Color.Red,
            }.WithCurrentTimestamp()
            .Build();

            await modlogChannel.SendMessageAsync("", embed: modlog);
        }

        [SlashCommand("list", "Return a list of all roles in the server.")]
        public async Task RoleListCommandAsync(){
            var title = $"Roles List - {Context.Guild.Roles.Count} total";
            var description = "";

            foreach (var role in Context.Guild.Roles){
                string roleInfo = $"* {role.Mention} - {role.Members.Count()} member(s)\n";
                description = description + roleInfo;
            }

            var list = new EmbedBuilder(){
                Title = title,
                Description = description,
                Color = Discord.Color.Blue,
            }.WithCurrentTimestamp()
            .Build();

            await RespondAsync("", embed: list);
        }

        [SlashCommand("info", "View a role and its members.")]
        public async Task RoleInfoCommandAsync(SocketRole role){
            var title = $"{role.Name} role";
            var description = $"**Mention**:{role.Mention}\n**Members:** {role.Members.Count()} member(s)\n**Color:** {role.Color.ToString}\n**Hoisted:** {role.IsHoisted}\n**Mentionable:** {role.IsMentionable}\n**Position**: {role.Position}\n\n**Members**\n\n";
        
            foreach (var member in role.Members){
                description = description + $"{member.Mention}\n";
            }

            var info = new EmbedBuilder(){
                Title = title,
                Description = description,
                Color = Discord.Color.Blue,
            }.WithCurrentTimestamp()
            .Build();

            await RespondAsync("", embed: info);
        }
    }

    //Moderation tools
    [SlashCommand("warn", "Send a warning to a member.")]
    public async Task WarnCommandAsync(IGuildUser member, string reason = "",
    [Summary(description:"Channel to send the warning to. Leaving this blank will only send the warning to the member's DMs.")]ITextChannel? channel = null){
        reason = string.IsNullOrWhiteSpace(reason) ? "No reason given" : reason;

        await _db.CreateWarningAsync(member, reason);

        var warnings = await _db.GetUserWarningsAsync(member);
        var warningCount = (warnings == null || warnings.Count == 0) ? "N/A" : warnings.Count.ToString();

        var thumbnailUrl = member.GetAvatarUrl();
        string title = "Warning";
        string description = $"You have received a warning from the {Context.Guild.Name} moderation team.\n\n**Reason:**\n{reason}\n\n\nYou now have {warningCount} active warnings.";
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
        description = $"**Action Type:** Warning\n\n**Issued by:** {Context.User.Username}\n\n**Issued to**: {member.Username}\n\n**Reason:**\n{reason}";
        var modlog = new EmbedBuilder(){
            ThumbnailUrl = thumbnailUrl,
            Title = title,
            Description = description,
            Color = color,
        }.WithCurrentTimestamp()
         .Build();

        await modlogChannel.SendMessageAsync("", embed: modlog);
    }

    [Group("warnings", "Manage warnings.")]
    public class WarningsSubGroup : SlashCommandModule{
        public WarningsSubGroup(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger){}

        [SlashCommand("all", "View a list of all active warnings.")]
        public async Task WarningsAllCommandAsync(){
            var warnings = await _db.GetGuildWarningsAsync(Context.Guild);
            if (warnings == null || warnings.Count == 0){
                var embed = new EmbedBuilder(){
                    Title = "No active warnings found."
                }.WithCurrentTimestamp()
                .Build();
                await RespondAsync("", embed: embed);
                return;
            }

            var title = $"{warnings.Count} active warnings found";
            var description = "";
            foreach (var warning in warnings){
                description = description + $"* {Context.Guild.GetUser(warning.UserId).Mention}\n**Created:** {warning.Created.AsLocalTime}\n**Reason:** {warning.Reason}\n";
            }

            var embeddedWarnings = new EmbedBuilder(){
                    Title = title,
                    Description = description,
                    Color = Discord.Color.Orange
                }.WithCurrentTimestamp()
                .Build();
            await RespondAsync("", embed: embeddedWarnings);
        }

        [SlashCommand("clear", "Clear a given member's active warnings.")]
        public async Task WarningsClearCommandAsync(IGuildUser member){
            var warnings = await _db.GetUserWarningsAsync(member);
            if (warnings == null || warnings.Count == 0){
                await RespondAsync("Member does not have any active warnings.", ephemeral: true);
                return;
            }

            await _db.ClearWarningsAsync(member);
            await RespondAsync($"{member.Mention}'s warnings have been cleared.");
        }

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
            Author = author,
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