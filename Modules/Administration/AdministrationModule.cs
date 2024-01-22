using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;


namespace DiscordBot.Modules.Administration
{
    //A module must be public and inherit ModuleBase in order to be discovered by AddModulesAsync.
    public class AdministrationModule : BaseModule
    {
        public AdministrationModule(ILogger<Bot> logger) : base(logger)
        {
        }
        
        [Command("announce")]
        [Summary("Sends an announcement to the target channel.")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task AnnounceCommandAsync(IChannel targetChannel, [Remainder] string announcementMessage){
            //The IChannel generic type does not contain SendMessageAsync, so an announcement channel is expected instead.
            INewsChannel announcementChannel;

            try{
                //Validate the target channel as an announcement channel and the message as a non-empty message.
                announcementChannel = targetChannel as INewsChannel ?? throw new ArgumentException("<channel> must be an announcement channel.");
                announcementMessage = string.IsNullOrWhiteSpace(announcementMessage)? throw new ArgumentException("<message> required.") : announcementMessage;
            }catch(ArgumentException exception)
            {
                //Handle the exception by logging the error and replying with correct usage information.
                _logger.LogError(exception, "Error on command execute: ");
                await ReplyAsync($"{exception.Message}\n\nUsage: !announce <channel> <message>");
                return;
            }

            await announcementChannel.SendMessageAsync(announcementMessage);
        }
    }

}
