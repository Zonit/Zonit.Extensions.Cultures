using Microsoft.Extensions.DependencyInjection;

namespace Zonit.Extensions.Cultures.Tests;
internal class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddCulturesExtension()
            .BuildServiceProvider();

        var cultureManager = serviceProvider.GetService<ICultureManager>();

        if (cultureManager is null)
            return;

        cultureManager.SetCulture("en-us");

        var supportedCultures = cultureManager.SupportedCultures;

        foreach (var culture in supportedCultures)
        {
            Console.WriteLine(culture.EnglishName);
        }
    }
}
