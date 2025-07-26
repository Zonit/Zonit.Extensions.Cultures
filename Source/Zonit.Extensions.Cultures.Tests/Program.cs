using Microsoft.Extensions.DependencyInjection;
using Zonit.Extensions.Cultures.Models;
using Zonit.Extensions.Cultures.Repositories;

namespace Zonit.Extensions.Cultures.Tests;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Zonit.Extensions.Cultures Test Suite ===");
        Console.WriteLine();

        // Setup service provider
        var serviceProvider = new ServiceCollection()
            .AddCulturesExtension()
            .BuildServiceProvider();

        // Test 1: Basic Culture Manager functionality
        TestCultureManager(serviceProvider);
        
        // Test 2: Translation functionality with new Translated value object
        TestTranslations(serviceProvider);
        
        // Test 3: Culture Provider functionality
        TestCultureProvider(serviceProvider);
        
        // Test 4: DateTime and timezone functionality
        TestDateTimeAndTimezone(serviceProvider);

        // Test 5: Translated value object functionality
        TestTranslatedValueObject();

        // Test 6: Missing translations recording
        TestMissingTranslationsRecording(serviceProvider);

        Console.WriteLine("\n=== All tests completed ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void TestCultureManager(ServiceProvider serviceProvider)
    {
        Console.WriteLine("1. Testing Culture Manager...");
        
        var cultureManager = serviceProvider.GetService<ICultureManager>();
        
        if (cultureManager == null)
        {
            Console.WriteLine("❌ CultureManager not registered properly");
            return;
        }

        // Test setting different cultures
        string[] testCultures = { "en-us", "pl-pl", "de-de", "fr-fr", "invalid-culture" };
        
        foreach (var culture in testCultures)
        {
            try
            {
                cultureManager.SetCulture(culture);
                Console.WriteLine($"✅ Culture set to: {cultureManager.GetCulture} (requested: {culture})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to set culture {culture}: {ex.Message}");
            }
        }

        // Test supported cultures
        var supportedCultures = cultureManager.SupportedCultures;
        Console.WriteLine($"✅ Supported cultures count: {supportedCultures.Length}");
        
        foreach (var culture in supportedCultures.Take(5))
        {
            Console.WriteLine($"   - {culture.Code}: {culture.EnglishName}");
        }

        // Test timezone
        try
        {
            cultureManager.SetTimeZone("Europe/London");
            Console.WriteLine($"✅ Timezone set to: {cultureManager.GetTimeZone}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to set timezone: {ex.Message}");
        }

        Console.WriteLine();
    }

    static void TestTranslations(ServiceProvider serviceProvider)
    {
        Console.WriteLine("2. Testing Translation functionality...");
        
        var translationRepo = serviceProvider.GetService<TranslationRepository>();
        var cultureProvider = serviceProvider.GetService<ICultureProvider>();
        
        if (translationRepo == null || cultureProvider == null)
        {
            Console.WriteLine("❌ Translation services not registered properly");
            return;
        }

        // Add some test translations
        var variable1 = new Variable("welcome_message", new List<Translate>
        {
            new() { Culture = "en-us", Content = "Welcome, {0}!" },
            new() { Culture = "pl-pl", Content = "Witaj, {0}!" },
            new() { Culture = "de-de", Content = "Willkommen, {0}!" }
        });

        var variable2 = new Variable("goodbye_message", new List<Translate>
        {
            new() { Culture = "en-us", Content = "Goodbye!" },
            new() { Culture = "pl-pl", Content = "Do widzenia!" }
        });

        translationRepo.Add(variable1);
        translationRepo.Add(variable2);

        // Test translations in different cultures
        string[] cultures = { "en-us", "pl-pl", "de-de", "fr-fr" };
        
        foreach (var culture in cultures)
        {
            var cultureManager = serviceProvider.GetService<ICultureManager>();
            cultureManager?.SetCulture(culture);
            
            Console.WriteLine($"Testing culture: {culture}");
            
            // Test with parameters
            Translated welcomeMsg = cultureProvider.Translate("welcome_message", "John");
            Console.WriteLine($"  Welcome: '{welcomeMsg}'");
            
            // Test without parameters
            Translated goodbyeMsg = cultureProvider.Translate("goodbye_message");
            Console.WriteLine($"  Goodbye: '{goodbyeMsg}'");
            
            // Test missing translation (should fallback)
            Translated missingMsg = cultureProvider.Translate("missing_key");
            Console.WriteLine($"  Missing: '{missingMsg}'");
            
            // Test with formatting
            Translated formattedMsg = cultureProvider.Translate("welcome_message", "User", "Extra");
            Console.WriteLine($"  Formatted: '{formattedMsg}'");
        }

        Console.WriteLine();
    }

    static void TestCultureProvider(ServiceProvider serviceProvider)
    {
        Console.WriteLine("3. Testing Culture Provider...");
        
        var cultureProvider = serviceProvider.GetService<ICultureProvider>();
        
        if (cultureProvider == null)
        {
            Console.WriteLine("❌ CultureProvider not registered properly");
            return;
        }

        // Test culture detection
        Console.WriteLine($"✅ Current culture: {cultureProvider.GetCulture}");
        
        // Test date time format
        var dateTimeFormat = cultureProvider.DateTimeFormat;
        Console.WriteLine($"✅ Date format: {dateTimeFormat.ShortDatePattern}");
        Console.WriteLine($"✅ Time format: {dateTimeFormat.ShortTimePattern}");

        // Test event handling - make sure we change to a different culture
        bool eventTriggered = false;
        cultureProvider.OnChange += () => 
        {
            eventTriggered = true;
            Console.WriteLine("✅ Culture change event triggered");
        };

        var cultureManager = serviceProvider.GetService<ICultureManager>();
        
        // Get current culture and change to a different one
        var currentCulture = cultureProvider.GetCulture;
        var newCulture = currentCulture == "en-us" ? "pl-pl" : "en-us";
        
        cultureManager?.SetCulture(newCulture);
        
        // Give a small delay to ensure event is processed
        Thread.Sleep(10);
        
        if (eventTriggered)
        {
            Console.WriteLine("✅ Event system working correctly");
        }
        else
        {
            Console.WriteLine("❌ Event system not working");
        }

        Console.WriteLine();
    }

    static void TestDateTimeAndTimezone(ServiceProvider serviceProvider)
    {
        Console.WriteLine("4. Testing DateTime and Timezone...");
        
        var cultureProvider = serviceProvider.GetService<ICultureProvider>();
        var cultureManager = serviceProvider.GetService<ICultureManager>();
        
        if (cultureProvider == null || cultureManager == null)
        {
            Console.WriteLine("❌ Services not registered properly");
            return;
        }

        var utcNow = DateTime.UtcNow;
        Console.WriteLine($"UTC time: {utcNow:yyyy-MM-dd HH:mm:ss}");

        // Test different timezones
        string[] timezones = { "Europe/Warsaw", "America/New_York", "Asia/Tokyo", "UTC" };
        
        foreach (var timezone in timezones)
        {
            try
            {
                cultureManager.SetTimeZone(timezone);
                var localTime = cultureProvider.ClientTimeZone(utcNow);
                Console.WriteLine($"✅ {timezone}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ {timezone}: {ex.Message}");
            }
        }

        Console.WriteLine();
    }

    static void TestTranslatedValueObject()
    {
        Console.WriteLine("5. Testing Translated Value Object...");

        // Test constructors and basic functionality
        Translated empty = Translated.Empty;
        Translated fromString = "Hello World";
        Translated fromConstructor = new Translated("Test Message");

        Console.WriteLine($"✅ Empty: '{empty}' (IsEmpty: {empty.IsEmpty})");
        Console.WriteLine($"✅ From string: '{fromString}' (IsEmpty: {fromString.IsEmpty})");
        Console.WriteLine($"✅ From constructor: '{fromConstructor}' (IsEmpty: {fromConstructor.IsEmpty})");

        // Test implicit conversions
        string stringValue = fromString; // Translated -> string
        Translated backToTranslated = stringValue; // string -> Translated
        Console.WriteLine($"✅ Conversion test: '{backToTranslated}'");

        // Test equality
        Translated msg1 = new Translated("Same");
        Translated msg2 = new Translated("Same");
        Translated msg3 = new Translated("Different");

        Console.WriteLine($"✅ Equality test: msg1 == msg2: {msg1 == msg2}");
        Console.WriteLine($"✅ Equality test: msg1 == msg3: {msg1 == msg3}");

        // Test properties
        Translated whitespace = new Translated("   ");
        Translated nullString = new Translated(null);
        
        Console.WriteLine($"✅ Whitespace IsNullOrWhiteSpace: {whitespace.IsNullOrWhiteSpace}");
        Console.WriteLine($"✅ Null IsEmpty: {nullString.IsEmpty}");

        // Test with actual translation methods
        Console.WriteLine($"✅ Value property: '{fromConstructor.Value}'");
        Console.WriteLine($"✅ ToString(): '{fromConstructor.ToString()}'");

        Console.WriteLine();
    }

    static void TestMissingTranslationsRecording(ServiceProvider serviceProvider)
    {
        Console.WriteLine("6. Testing Missing Translations Recording...");

        var cultureProvider = serviceProvider.GetService<ICultureProvider>();
        var cultureManager = serviceProvider.GetService<ICultureManager>();
        var missingRepo = serviceProvider.GetService<MissingTranslationRepository>();

        if (cultureProvider == null || cultureManager == null || missingRepo == null)
        {
            Console.WriteLine("❌ Required services not registered properly");
            return;
        }

        // Clear any existing missing translations
        missingRepo.Clear();
        Console.WriteLine("✅ Cleared existing missing translations");

        // Test 1: Missing translation for non-default culture
        cultureManager.SetCulture("pl-pl");
        var missingKey1 = "missing_key_polish";
        var result1 = cultureProvider.Translate(missingKey1);
        
        Console.WriteLine($"✅ Requested missing key '{missingKey1}' in pl-pl, got: '{result1}'");

        // Test 2: Missing translation for another culture
        cultureManager.SetCulture("de-de");
        var missingKey2 = "missing_key_german";
        var result2 = cultureProvider.Translate(missingKey2, "param1", "param2");
        
        Console.WriteLine($"✅ Requested missing key '{missingKey2}' in de-de with params, got: '{result2}'");

        // Test 3: Same key but different culture
        cultureManager.SetCulture("fr-fr");
        var result3 = cultureProvider.Translate(missingKey1); // Same key as test 1
        
        Console.WriteLine($"✅ Requested same key '{missingKey1}' in fr-fr, got: '{result3}'");

        // Test 4: Default culture (should NOT be recorded)
        cultureManager.SetCulture("en-us");
        var missingKey3 = "missing_key_english";
        var result4 = cultureProvider.Translate(missingKey3);
        
        Console.WriteLine($"✅ Requested missing key '{missingKey3}' in en-us (default), got: '{result4}'");

        // Check what was recorded
        var missingTranslations = missingRepo.GetAll();
        Console.WriteLine($"\n📝 Missing translations recorded: {missingTranslations.Count}");

        foreach (var missing in missingTranslations)
        {
            Console.WriteLine($"   🔍 Key: '{missing.Name}'");
            if (missing.Translates != null)
            {
                foreach (var translate in missing.Translates)
                {
                    Console.WriteLine($"      - Culture: '{translate.Culture}', Content: '{translate.Content}'");
                }
            }
        }

        // Verify expected behavior
        var expectedKeys = new[] { missingKey1, missingKey2 }; // missingKey3 should NOT be recorded (default culture)
        var actualKeys = missingTranslations.Select(m => m.Name).ToArray();

        bool allExpectedFound = expectedKeys.All(key => actualKeys.Contains(key));
        bool noUnexpectedFound = !actualKeys.Contains(missingKey3); // English key should not be recorded

        if (allExpectedFound && noUnexpectedFound)
        {
            Console.WriteLine("✅ Missing translation recording works correctly!");
            
            // Check if multiple cultures are recorded for the same key
            var missingKey1Record = missingTranslations.FirstOrDefault(m => m.Name == missingKey1);
            if (missingKey1Record?.Translates?.Count >= 2)
            {
                Console.WriteLine("✅ Multiple cultures recorded for the same missing key!");
            }
        }
        else
        {
            Console.WriteLine("❌ Missing translation recording has issues");
            if (!allExpectedFound)
            {
                Console.WriteLine($"   Missing expected keys: {string.Join(", ", expectedKeys.Except(actualKeys))}");
            }
            if (!noUnexpectedFound)
            {
                Console.WriteLine($"   Found unexpected key: {missingKey3} (default culture should not be recorded)");
            }
        }

        Console.WriteLine();
    }
}
