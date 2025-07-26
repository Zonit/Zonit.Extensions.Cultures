using System.Globalization;
using Zonit.Extensions.Cultures;
using Zonit.Extensions.Cultures.Models;
using Zonit.Extensions.Cultures.Repositories;

namespace Zonit.Extensions.Cultures.Services;

public class CultureService : ICultureProvider
{
    private const string DefaultCulture = "en-us";
    
    private readonly TranslationRepository _translationRepository;
    private readonly MissingTranslationRepository _missingTranslationRepository;
    private readonly ICultureManager _cultureManager;

    private DateTimeFormatModel _dateTimeFormat;

    public CultureService(
        TranslationRepository translationRepository,
        MissingTranslationRepository missingTranslationRepository,
        ICultureManager cultureManager)
    {
        _translationRepository = translationRepository ?? throw new ArgumentNullException(nameof(translationRepository));
        _missingTranslationRepository = missingTranslationRepository ?? throw new ArgumentNullException(nameof(missingTranslationRepository));
        _cultureManager = cultureManager ?? throw new ArgumentNullException(nameof(cultureManager));

        _cultureManager.OnChange += HandleCultureManagerChange;
        
        UpdateCultureProperties();
    }

    public DateTimeFormatModel DateTimeFormat => _dateTimeFormat;
    public string GetCulture { get; private set; } = DefaultCulture;
    public event Action? OnChange;

    private void HandleCultureManagerChange()
    {
        UpdateCultureProperties();
        OnChange?.Invoke();
    }

    private void UpdateCultureProperties()
    {
        GetCulture = NormalizeCultureCode(_cultureManager.GetCulture);
        
        _dateTimeFormat = new DateTimeFormatModel
        {
            ShortDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
            ShortTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
        };
    }

    private static string NormalizeCultureCode(string? culture)
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

    private static bool AreCulturesEqual(string? culture1, string? culture2)
    {
        return string.Equals(
            NormalizeCultureCode(culture1), 
            NormalizeCultureCode(culture2), 
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDefaultCulture(string? culture)
    {
        return AreCulturesEqual(culture, DefaultCulture);
    }

    public string Translate(string content, params object?[] args)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        try
        {
            var currentCulture = GetCulture;
            var translation = FindTranslation(content, currentCulture);

            if (translation != null)
            {
                return FormatTranslation(translation.Content, args);
            }

            // Fallback to default culture if not found
            if (!IsDefaultCulture(currentCulture))
            {
                var defaultTranslation = FindTranslation(content, DefaultCulture);
                if (defaultTranslation != null)
                {
                    return FormatTranslation(defaultTranslation.Content, args);
                }
            }

            // Record missing translation for development purposes
            RecordMissingTranslation(content, currentCulture);

            // Return original content as fallback
            return FormatTranslation(content, args);
        }
        catch (Exception)
        {
            // Log exception in production, for now return fallback
            return FormatTranslation(content, args);
        }
    }

    private Translate? FindTranslation(string content, string culture)
    {
        var variable = _translationRepository.GetAll()
            .FirstOrDefault(x => string.Equals(x.Name, content, StringComparison.Ordinal));

        return variable?.Translates?
            .FirstOrDefault(t => AreCulturesEqual(t.Culture, culture));
    }

    private static string FormatTranslation(string content, params object?[] args)
    {
        if (args == null || args.Length == 0)
            return content;

        try
        {
            return string.Format(content, args);
        }
        catch (FormatException)
        {
            // If formatting fails, return the original content
            return content;
        }
    }

    private void RecordMissingTranslation(string content, string culture)
    {
        // Skip recording for default culture to avoid noise
        if (IsDefaultCulture(culture))
            return;

        try
        {
            var existingMissing = _missingTranslationRepository.GetAll()
                .FirstOrDefault(x => string.Equals(x.Name, content, StringComparison.Ordinal));

            if (existingMissing != null)
            {
                // Add missing culture if not already recorded
                var existingTranslate = existingMissing.GetTranslate(culture);
                if (existingTranslate == null)
                {
                    var newTranslate = new Translate 
                    { 
                        Content = string.Empty, 
                        Culture = culture 
                    };
                    existingMissing.AddTranslate(newTranslate);
                }
            }
            else
            {
                // Create new missing translation record
                var newTranslate = new Translate 
                { 
                    Content = string.Empty, 
                    Culture = culture 
                };
                var missingVariable = new Variable(content, new List<Translate> { newTranslate });
                
                _missingTranslationRepository.Add(missingVariable);
            }
        }
        catch (Exception)
        {
            // Log exception in production, but don't fail the translation
        }
    }

    public DateTime ClientTimeZone(DateTime utcDateTime)
    {
        try
        {
            var timeZoneId = _cultureManager.GetTimeZone;
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return utcDateTime;

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZone);
        }
        catch (Exception)
        {
            // Log exception in production
            // Return UTC time as fallback
            return utcDateTime;
        }
    }
}