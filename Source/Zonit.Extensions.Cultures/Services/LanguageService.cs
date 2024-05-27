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
        if (languages.TryGetValue(name, out LanguageModel? language))
        {
            return language;
        }
        else
        {
            throw new ArgumentException($"Language with name '{name}' not found.");
        }
    }
}
