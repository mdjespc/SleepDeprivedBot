using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
//using DiscordBot.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules.SlashCommands;

public class AdminSlashModule : SlashCommandModule{
    public AdminSlashModule(ILogger<Bot> logger) : base(logger) {}

    

}