using Zonit.Extensions.Cultures;
using Zonit.Extensions.Cultures.Services;

namespace Zonit.Extensions.Cultures.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var languageProvider = new LanguageService();

            Console.WriteLine(languageProvider.GetByCode("pl-pl").IconFlag);
        }
    }
}
