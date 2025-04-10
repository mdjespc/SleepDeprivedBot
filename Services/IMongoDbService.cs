namespace DiscordBot.Services{
    public interface IMongoDbService{
        Task SetGuildSettingsAsync(ulong guildId, string key, string value);
        Task<GuildSettingsModel> GetGuildSettingsAsync(ulong guildId);
    }
}