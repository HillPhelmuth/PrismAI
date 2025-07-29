using Microsoft.JSInterop;
using PrismAI.Core.Models.CultureConciergeModels;

namespace PrismAI.Components;


public class PrismAIJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public PrismAIJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/PrismAI.Components/prismAIJsInterop.js").AsTask());
    }

    public async ValueTask<GeolocationCoordinates?> GetCurrentLocation()
    {
        var item = await (await moduleTask.Value).InvokeAsync<string>("getLocation");
        try
        {
            Console.WriteLine($"Location data:\n=============================\n{item}");
            var coordinates = System.Text.Json.JsonSerializer.Deserialize<GeolocationCoordinates>(item);
            return coordinates;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting location: {ex.Message}");
            return null;
        }
    }

    public async ValueTask ScrollToTop()
    {
        await (await moduleTask.Value).InvokeVoidAsync("scrollToTop");
    }
    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}