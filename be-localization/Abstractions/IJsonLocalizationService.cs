namespace be_localization.Abstractions
{
    public interface IJsonLocalizationService
    {
        string Get<TEnum>(TEnum key, params object[] args) where TEnum : Enum;
        string this[Enum key] { get; }
        string Get<TEnum>(string prefix, TEnum key, params object[] args) where TEnum : Enum;

    }
}
