﻿namespace Zonit.Extensions.Cultures;

/// <summary>
/// Manager to manage the state of culture 
/// </summary>
public interface ICultureManager
{
    /// <summary>
    /// Get the current culture in use in the BCP 47 standard
    /// </summary>
    public string GetCulture { get; }

    /// <summary>
    /// Get the current time zone in use in the IANA standard
    /// </summary>
    public string GetTimeZone { get; }

    /// <summary>
    /// Get list of supported cultures
    /// </summary>
    public LanguageModel[] SupportedCultures { get; }

    /// <summary>
    /// Changing the default language
    /// </summary>
    /// <param name="culture">Language parameter in BCP 47 standard</param>
    public void SetCulture(string culture);

    /// <summary>
    /// Changing the default time zone
    /// </summary>
    /// <param name="timeZone"></param>
    public void SetTimeZone(string timeZone);

    /// <summary>
    /// Event that is triggered when the language is changed
    /// </summary>
    public event Action? OnChange;
}
