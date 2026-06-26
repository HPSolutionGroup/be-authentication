using System.Text.Json.Serialization;

namespace HP.Authentication.Localization.Extensions
{
    public class LocalizationConfig
    {
        [JsonPropertyName("defaultLanguage")]
        public string DefaultLanguage { get; set; } = "vi";

        [JsonPropertyName("supportedLanguages")]
        public List<string> SupportedLanguages { get; set; } = ["vi"];
    }
}
