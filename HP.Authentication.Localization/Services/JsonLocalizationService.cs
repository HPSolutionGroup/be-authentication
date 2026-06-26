using HP.Authentication.Localization.Abstractions;
using HP.Authentication.Localization.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace HP.Authentication.Localization.Services
{
    public class JsonLocalizationService : IJsonLocalizationService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, Dictionary<string, string>> _localizationCache = new();
        private LocalizationConfig _config = new();

        public JsonLocalizationService(IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            LoadConfig();
            LoadAllLanguages();
        }

        public string this[Enum key] => Get(key.ToString());

        public string Get<TEnum>(TEnum key, params object[] args) where TEnum : Enum =>
            Get(key.ToString(), args);

        public string Get<TEnum>(string prefix, TEnum key, params object[] args) where TEnum : Enum =>
            Get($"{prefix}.{key}", args);

        public string Get(string key, params object[] args)
        {
            var culture = GetRequestCulture();

            if (_localizationCache.TryGetValue(culture, out var dict) &&
                dict.TryGetValue(key, out var value))
            {
                return string.Format(value, args);
            }

            return key; // fallback
        }

        public string GetRequestCulture()
        {
            var culture = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();

            if (string.IsNullOrWhiteSpace(culture))
                return _config.DefaultLanguage;

            //var lang = culture.Split(',')[0].Trim().ToLower();
            var lang = culture.Split(',')[0].Trim().ToLower().Split('-')[0];

            if (_config.SupportedLanguages.Contains(
                    lang,
                    StringComparer.OrdinalIgnoreCase))
            {
                return lang;
            }

            return _config.DefaultLanguage;
        }

        private void LoadConfig()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "config", "localization.config.json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Localization config not found: {path}");
            }

            var json = File.ReadAllText(path);

            _config = JsonSerializer.Deserialize<LocalizationConfig>(json)
                      ?? new LocalizationConfig();
        }
        private void LoadAllLanguages()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", "i18n");
            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.GetFiles(path, "*.json"))
            {
                var culture = Path.GetFileNameWithoutExtension(file).ToLower();
                var json = File.ReadAllText(file);

                var element = JsonSerializer.Deserialize<JsonElement>(json);
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var flatDict = new Dictionary<string, string>();
                    FlattenJson(element, "", flatDict);
                    _localizationCache[culture] = flatDict;
                }
            }
            ValidateConfig();
        }

        private void FlattenJson(JsonElement element, string parentKey, Dictionary<string, string> result)
        {
            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(parentKey) ? property.Name : $"{parentKey}.{property.Name}";

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    FlattenJson(property.Value, key, result);
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    result[key] = property.Value.GetString()!;
                }
            }
        }

        private void ValidateConfig()
        {
            if (!_config.SupportedLanguages.Any())
            {
                throw new InvalidOperationException(
                    "SupportedLanguages cannot be empty.");
            }
            foreach (var language in _config.SupportedLanguages)
            {
                if (!_localizationCache.ContainsKey(language))
                {
                    throw new InvalidOperationException(
                        $"Language file '{language}.json' not found.");
                }
            }

            if (!_config.SupportedLanguages.Contains(_config.DefaultLanguage))
            {
                throw new InvalidOperationException(
                    $"Default language '{_config.DefaultLanguage}' must exist in supportedLanguages.");
            }
        }
    }

}
