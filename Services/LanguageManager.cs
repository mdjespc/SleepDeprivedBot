

using System.Text.Json;

namespace DiscordBot.Services{
    public class LanguageManager : ILanguageManager{
        private readonly Dictionary<string, Dictionary<string, string>> _languages;

        public LanguageManager(){
            _languages = new Dictionary<string, Dictionary<string, string>>();
            LoadLanguages();
        }

        private void LoadLanguages(){
            foreach(var file in Directory.GetFiles("Languages", "*.json")){
                var code = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                _languages[code] = dict ?? new Dictionary<string, string>();
            }
        }

        public string GetString(string key, string langCode = "en", params object[] args){
            if (!_languages.ContainsKey(langCode))
                langCode = "en";
            
            var dict = _languages[langCode];

            if(!dict.TryGetValue(key, out var value))
                value = $"[MISSING: {key}]";

            return string.Format(value, args);
        }
    }
}