using Discord;
using Discord.Interactions;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
//using DiscordBot.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules.SlashCommands;

public class AdminSlashModule : SlashCommandModule{
    public AdminSlashModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger) {}

    

}