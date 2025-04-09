using Discord;
using Discord.Interactions;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
//using DiscordBot.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules.SlashCommands;

public class AdminSlashModule : SlashCommandModule{
    public AdminSlashModule(IMongoDbService db, ILogger<Bot> logger) : base(db, logger) {}

    

}