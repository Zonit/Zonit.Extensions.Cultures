using Zonit.Extensions.Cultures.Languages;

namespace Zonit.Extensions.Cultures.Services;

public class LanguageService : ILanguageProvider
{
    readonly Dictionary<string, LanguageModel> languages = [];

    // FIXME: Dodaj codegenerator, niech się generuje kod przy kompilacji
    public LanguageService()
    {
        RegisterLanguage(new Arabic());
        RegisterLanguage(new Czech());
        RegisterLanguage(new Danish());
        RegisterLanguage(new Dutch());
        RegisterLanguage(new English());
        RegisterLanguage(new Finnish());
        RegisterLanguage(new French());
        RegisterLanguage(new German());
        RegisterLanguage(new Hungarian());
        RegisterLanguage(new Italian());
        RegisterLanguage(new Norwegian());
        RegisterLanguage(new Polish());
        RegisterLanguage(new Portuguese());
        RegisterLanguage(new Russian());
        RegisterLanguage(new Slovak());
        RegisterLanguage(new Spanish());
        RegisterLanguage(new Swedish());
    }

    private void RegisterLanguage(LanguageModel language)
    {
        languages[language.Code] = language;
    }

    public LanguageModel GetByCode(string name)
    {
        // Najpierw próba bezpośredniego znalezienia kodu języka
        if (languages.TryGetValue(name.ToLower(), out LanguageModel? language))
        {
            return language;
        }

        // Próba znalezienia głównego kodu języka (np. "en" dla "en-gb")
        string mainLanguageCode = name.Split('-')[0].ToLower();
        var mainLanguage = languages.FirstOrDefault(l => l.Key.StartsWith($"{mainLanguageCode}-"));

        if (mainLanguage.Value != null)
        {
            // Logowanie informacji o użyciu zamiennika
            // logger?.LogWarning($"Language '{name}' not found, using '{mainLanguage.Key}' instead.");
            return mainLanguage.Value;
        }

        // Domyślny język (angielski)
        if (languages.TryGetValue("en-us", out LanguageModel? defaultLanguage))
        {
            // Logowanie informacji o użyciu domyślnego języka
            // logger?.LogWarning($"Language '{name}' not found, using default 'en-us' instead.");
            return defaultLanguage;
        }

        // Ostatecznie, jeśli nawet domyślny język nie jest dostępny, rzuć wyjątek
        throw new ArgumentException($"Language with name '{name}' not found, and default language is not available.");
    }
}
