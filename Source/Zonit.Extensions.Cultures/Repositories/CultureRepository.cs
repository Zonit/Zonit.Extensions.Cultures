using System.Globalization;

namespace Zonit.Extensions.Cultures.Repositories;

internal class CultureRepository(ILanguageProvider languageProvider) : ICultureManager
{
    private const string DefaultCulture = "en-us";
    private const string DefaultTimeZone = "Europe/Warsaw";
    
    private string _culture = DefaultCulture;
    private string _timeZone = DefaultTimeZone;
    
    private readonly string[] _supportedCultures = [
        "en-us",
        "ar-sa",
        "fr-fr",
        "de-de",
        "es-es",
        "it-it",
        "nl-nl",
        "sv-se",
        "da-dk",
        "no-no",
        "fi-fi",
        "ru-ru",
        "pl-pl",
        "cs-cz",
        "hu-hu",
        "sk-sk",
        "pt-pt"
    ];

    private List<LanguageModel> _supportedCulturesModel = [];

    public string GetCulture => _culture;
    public string GetTimeZone => _timeZone;
    public LanguageModel[] SupportedCultures => _supportedCulturesModel.ToArray();
    public event Action? OnChange;

    private static string NormalizeCultureCode(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return DefaultCulture;

        try
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            return cultureInfo.Name.ToLowerInvariant();
        }
        catch (CultureNotFoundException)
        {
            return DefaultCulture;
        }
    }

    public void SetCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = DefaultCulture;
        }

        try
        {
            var normalizedCulture = NormalizeCultureCode(culture);
            
            // Only update if culture actually changed
            if (string.Equals(_culture, normalizedCulture, StringComparison.OrdinalIgnoreCase))
                return;

            _culture = normalizedCulture;

            // Update supported cultures model
            RefreshSupportedCulturesModel();

            // Set system culture
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            OnChange?.Invoke();
        }
        catch (Exception)
        {
            // Log exception in production
            // Fallback to default culture
            _culture = DefaultCulture;
            RefreshSupportedCulturesModel();
            OnChange?.Invoke();
        }
    }

    public void SetTimeZone(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            timeZone = DefaultTimeZone;
        }

        try
        {
            // Validate the time zone
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            
            // Only update if timezone actually changed
            if (string.Equals(_timeZone, timeZone, StringComparison.OrdinalIgnoreCase))
                return;

            _timeZone = timeZone;
            OnChange?.Invoke();
        }
        catch (Exception)
        {
            // Log exception in production
            // Fallback to default timezone
            _timeZone = DefaultTimeZone;
            OnChange?.Invoke();
        }
    }

    private void RefreshSupportedCulturesModel()
    {
        _supportedCulturesModel.Clear();
        
        foreach (var cultureCode in _supportedCultures)
        {
            try
            {
                var languageModel = languageProvider.GetByCode(cultureCode);
                if (languageModel != null)
                {
                    _supportedCulturesModel.Add(languageModel);
                }
            }
            catch (Exception)
            {
                // Log exception in production
                // Skip this culture and continue
            }
        }
    }
}

// TODO: Warto ogarnąć
// https://learn.microsoft.com/pl-pl/aspnet/core/blazor/globalization-localization?view=aspnetcore-8.0#dynamically-set-the-client-side-culture-by-user-preference