using System.Globalization;
using Microsoft.Extensions.Options;
using Zonit.Extensions.Cultures.Options;

namespace Zonit.Extensions.Cultures.Repositories;

internal class CultureRepository : ICultureManager
{
    private readonly ILanguageProvider languageProvider;
    private readonly CultureOption _cultureOptions;
    
    private string _culture;
    private string _timeZone;
    
    private List<LanguageModel> _supportedCulturesModel = [];

    public string GetCulture => _culture;
    public string GetTimeZone => _timeZone;
    public LanguageModel[] SupportedCultures => _supportedCulturesModel.ToArray();
    public event Action? OnChange;

    public CultureRepository(ILanguageProvider languageProvider, IOptions<CultureOption> options)
    {
        this.languageProvider = languageProvider;
        _cultureOptions = options.Value;
        
        _culture = NormalizeCultureCode(_cultureOptions.DefaultCulture);
        _timeZone = _cultureOptions.DefaultTimeZone;
        
        InitializeSupportedCultures();
    }

    private static string NormalizeCultureCode(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return "en-us";

        try
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            return cultureInfo.Name.ToLowerInvariant();
        }
        catch (CultureNotFoundException)
        {
            return "en-us";
        }
    }

    public void SetCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            culture = _cultureOptions.DefaultCulture;
        }

        try
        {
            var normalizedCulture = NormalizeCultureCode(culture);
            
            // Check if culture is supported
            if (!_cultureOptions.SupportedCultures.Contains(normalizedCulture, StringComparer.OrdinalIgnoreCase))
            {
                normalizedCulture = NormalizeCultureCode(_cultureOptions.DefaultCulture);
            }
            
            // Only update if culture actually changed
            if (string.Equals(_culture, normalizedCulture, StringComparison.OrdinalIgnoreCase))
                return;

            _culture = normalizedCulture;

            // Set system culture
            var cultureInfo = CultureInfo.GetCultureInfo(_culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            OnChange?.Invoke();
        }
        catch (Exception)
        {
            // Log exception in production
            // Fallback to default culture
            _culture = NormalizeCultureCode(_cultureOptions.DefaultCulture);
            OnChange?.Invoke();
        }
    }

    public void SetTimeZone(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            timeZone = _cultureOptions.DefaultTimeZone;
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
            _timeZone = _cultureOptions.DefaultTimeZone;
            OnChange?.Invoke();
        }
    }

    private void InitializeSupportedCultures()
    {
        _supportedCulturesModel.Clear();
        
        foreach (var cultureCode in _cultureOptions.SupportedCultures)
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