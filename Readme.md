## Supporting language culture

Advanced language support for Blazor. 

**Nuget Package Abstraction**
```
Install-Package Zonit.Extensions.Cultures.Abstractions 
```
![NuGet Version](https://img.shields.io/nuget/v/Zonit.Extensions.Cultures.Abstractions.svg)
![NuGet](https://img.shields.io/nuget/dt/Zonit.Extensions.Cultures.Abstractions.svg)


**Nuget Package Extensions**
```
Install-Package Zonit.Extensions.Cultures
```
![NuGet Version](https://img.shields.io/nuget/v/Zonit.Extensions.Cultures.svg)
![NuGet](https://img.shields.io/nuget/dt/Zonit.Extensions.Cultures.svg)

**Install**
Add this in ``Routes.razor``
```razor
@using Zonit.Extensions

<ZonitCulturesExtension />
```

Services in ``Program.cs``
```cs
builder.Services.AddCulturesExtension();
```
App in ``Program.cs``
```cs
app.UseCulturesExtension();
```

### Example usage:

```razor
@page "/culture"
@using Zonit.Extensions.Cultures
@implements IDisposable
@inject ICultureProvider Culture

@Culture.Translate("Hello {0}", "user")

@code{
    protected override void OnInitialized()
    {
        Culture.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        Culture.OnChange -= StateHasChanged;
    }
}
```

### API:
```cs
public interface ICultureProvider
{
    /// <summary>
    /// Returns the current culture in use in the BCP 47 standard
    /// </summary>
    public string GetCulture { get; }

    /// <summary>
    /// Returns the translation in the current language used
    /// </summary>
    /// <param name="content">Search string, example: “Hello {0}”</param>
    /// <param name="args">Additional arguments, example: “User”</param>
    /// <returns></returns>
    public string Translate(string content, params object?[] args);

    /// <summary>
    /// Default time zone for the user
    /// </summary>
    /// <param name="utcDateTime"></param>
    /// <returns></returns>
    public DateTime ClientTimeZone(DateTime utcDateTime);

    /// <summary>
    /// Event that is triggered when the language is changed
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Date and time format
    /// </summary>
    public DateTimeFormatModel DateTimeFormat { get; }
}
```

```cs
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
```

```cs
public interface ITranslationManager
{
    /// <summary>
    /// Add one translation
    /// </summary>
    /// <param name="item"></param>
    public void Add(Variable item);

    /// <summary>
    /// Add multiple translations
    /// </summary>
    /// <param name="items"></param>
    public void AddRange(List<Variable> items);
}
```