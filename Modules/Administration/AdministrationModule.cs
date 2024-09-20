using System.Drawing;
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
                announcementMessage = string.IsNullOrWhiteSpace(announcementMessage) ? throw new ArgumentException("<message> required.") : announcementMessage;
            }catch(ArgumentException exception)
            {
                //Handle the exception by logging the error and replying with correct usage information.
                _logger.LogError(exception, "Error on command execute: ");
                await ReplyAsync($"{exception.Message}\n\nUsage: !announce <channel> <message>");
                return;
            }

            await announcementChannel.SendMessageAsync(announcementMessage);
        }


        [Command("sm")]
        [Summary("Sends a text message to the target channel.")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task SendMessageAsync(IChannel targetChannel, [Remainder] string textMessage){
            ITextChannel textChannel;

            try{
                textChannel = targetChannel as ITextChannel ?? throw new ArgumentException("<channel> must be a text channel.");
                textMessage = string.IsNullOrWhiteSpace(textMessage) ? throw new ArgumentException("<message> required.") : textMessage; 
            }catch(ArgumentException exception){
                _logger.LogError(exception, "Error on command execute: ");
                await ReplyAsync($"{exception.Message}\n\nUsage !sm <channel> <message>");
                return;
            }
            
            await textChannel.SendMessageAsync(textMessage);
        }



        /*
        Builds an embedded message and sends it to the target channel. The contents of the embed elements are parsed
        through a specified delimiter (;) in the following order:

        "title";"description"

        Embed elements: Color, Title, URL, Author, Description, Thumbnails, Fields, Image, Timestamp, Footer
        API Reference on Embed Builder: https://docs.discordnet.dev/api/Discord.EmbedBuilder.html
        */
        [Command("se")]
        [Summary("Sends a text message to the target channel.")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task SendEmbedAsync(IChannel targetChannel, [Remainder] string remainder){
            ITextChannel textChannel;

            try{
                textChannel = targetChannel as ITextChannel ?? throw new ArgumentException("<channel> must be a text channel.");
                remainder = string.IsNullOrWhiteSpace(remainder) ? throw new ArgumentException("<embed>") : remainder;
            }catch(ArgumentException exception){
                _logger.LogError(exception, "Error on command execute: ");
                await ReplyAsync($"{exception.Message}\n\nUsage !se <channel> <embed>");
                return;
            }

            List<string> embedElements = new List<string>();
            string title;
            string description;
            string delimiter = ";";

            try{
            Array.ForEach(remainder.Split(delimiter), embedElements.Add);
            if (embedElements.Count != 2){
                throw new ArgumentException($"Not enough embed arguments passed. Expected 2 (title, description), received {embedElements.Count}");
            }
            }catch (ArgumentException exception){
                _logger.LogError(exception, "Error on command execute: ");
                await ReplyAsync($"{exception.Message}");
                return;
            }
            
            title = embedElements.ElementAt(0);
            description = embedElements.ElementAt(1);
            EmbedBuilder embedBuilder = new EmbedBuilder() {
                Title = title,
                Description = description
            };


            await textChannel.SendMessageAsync("", false, embedBuilder.Build());

        }
    }

}
