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

[InteractionFramework.Attributes.RequireOwner]
public class OwnerSlashModule : SlashCommandModule
{
    public OwnerSlashModule(IMongoDbService db, ILanguageManager langManager, ILogger<Bot> logger) : base(db, langManager, logger) { }

    [SlashCommand("shutdown", "Shut down this bot.")]
    public async Task ShutdownCommandAsync()
    {
        _logger.LogInformation("Shutting down...");
        await RespondAsync("Shutting down...");
        await Context.Client.StopAsync();
        Environment.Exit(0);
        return;
    }

    [SlashCommand("leave", "Leave this server.")]
    public async Task LeaveCommandAsync()
    {
        _logger.LogInformation($"Left {Context.Guild.Name}");
        await Context.Guild.LeaveAsync();
    }

    //TODO alert command
} 