using Microsoft.AspNetCore.Components;
using Zonit.Extensions.Cultures;

namespace Zonit.Extensions;

public sealed class ZonitCulturesExtension : ComponentBase, IDisposable
{
    [Inject]
    ICultureManager Culture { get; set; } = default!;

    [Inject]
    PersistentComponentState ApplicationState { get; set; } = default!;

    string CultureName { get; set; } = null!;
    PersistingComponentStateSubscription persistingSubscription;

    protected override void OnInitialized()
    {
        persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

        if (!ApplicationState.TryTakeFromJson<string>("ZonitCulturesExtension", out var restored))
            CultureName = Culture.GetCulture;
        else
            CultureName = restored!;

        Culture.SetCulture(CultureName);
    }

    private Task PersistData()
    {
        ApplicationState.PersistAsJson("ZonitCulturesExtension", CultureName);

        return Task.CompletedTask;
    }

    public void Dispose()
        => persistingSubscription.Dispose();
}