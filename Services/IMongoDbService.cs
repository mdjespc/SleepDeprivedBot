using Discord;

namespace DiscordBot.Services;
    public interface IMongoDbService{
        Task SetGuildSettingsAsync(ulong guildId, string key, string value);
        Task<GuildSettingsModel> GetGuildSettingsAsync(ulong guildId);
        Task CreateGuildUserAsync(IGuildUser user);
        //Task UpdateGuildUserAsync(IGuildUser user, int? level = null, int? experience = null, int? currency = null){}
        Task DeleteGuildUserAsync(IGuildUser user);
        Task CreateWarningAsync(IGuildUser user, string? reason = null, double duration = 0);
        Task<List<WarningModel>?> GetUserWarningsAsync(IGuildUser user);
        Task<List<WarningModel>?> GetGuildWarningsAsync(IGuild guild);
        //Task UpdateWarningAsync(IGuildUser user, string? reason = null, int? duration = null){}
        Task DeleteWarningAsync(string id);
        Task ClearWarningsAsync(IGuildUser user);
}