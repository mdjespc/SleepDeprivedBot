namespace DiscordBot.Services{
    public interface ILanguageManager{
        string GetString(string key, string langCode, params object[] args);
    }
}