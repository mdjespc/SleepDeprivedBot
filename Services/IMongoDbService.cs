namespace DiscordBot.Services{
    public interface IMongoDbService{
        Task<GuildSettingsModel> GetGuildSettingsAsync(ulong guildId);
    }
}