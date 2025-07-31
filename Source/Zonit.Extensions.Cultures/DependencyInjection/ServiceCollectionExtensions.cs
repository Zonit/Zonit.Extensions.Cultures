using Microsoft.Extensions.DependencyInjection;
using Zonit.Extensions.Cultures;
using Zonit.Extensions.Cultures.Options;
using Zonit.Extensions.Cultures.Repositories;
using Zonit.Extensions.Cultures.Services;

namespace Zonit.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCulturesExtension(this IServiceCollection services, Action<CultureOption>? options = null)
    {
        services.AddOptions<CultureOption>()
                .BindConfiguration("Culture");

        if (options is not null)
            services.PostConfigure(options);

        services.AddSingleton<TranslationRepository>();
        services.AddSingleton<DefaultTranslationRepository>();
        services.AddSingleton<MissingTranslationRepository>();

        services.AddSingleton<ITranslationManager, TranslationService>();

        services.AddSingleton<DetectCultureService>();

        services.AddScoped<ICultureManager, CultureRepository>();
        services.AddScoped<ICultureProvider, CultureService>();

        services.AddTransient<ILanguageProvider, LanguageService>();

        return services;
    }
}