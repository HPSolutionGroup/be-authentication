using be_localization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace be_localization.Services
{
    public class JsonLocalizationService : IJsonLocalizationService
    {
        private readonly IHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Dictionary<string, Dictionary<string, string>> _localizationCache = new();

        public JsonLocalizationService(IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
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

        private string GetRequestCulture()
        {
            var culture = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(culture)) return "vi";

            var lang = culture.Split(',')[0].Trim().ToLower();
            return lang switch
            {
                "vi-vn" => "vi",
                "en-us" => "en",
                "vi" => "vi",
                "en" => "en",
                _ => "vi" // fallback nếu không khớp
            };
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
    }

}
