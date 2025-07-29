using System.Text.Json;
using Microsoft.JSInterop;
using PrismAI.Core.Models;
using PrismAI.Core.Models.CultureConciergeModels;
using Location = PrismAI.Core.Models.ResponseModels.Location;

namespace PrismAI.Maps;
// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class HeatMapJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
        "import", "./_content/PrismAI.Maps/heatMapInterop.js").AsTask());

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask GenerateHeatmap(HeatmapResult result)
    {
        var module = await moduleTask.Value;
        var json = JsonSerializer.Serialize(result);
        Console.WriteLine($"Heatmap data: {json[..1000]}");
        await module.InvokeVoidAsync("initHeatmap", json);
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

    public async ValueTask GenerateRoutesMap(Location destination)
    {
        var origin = await GetCurrentLocation();
        var originObj = new { lat = origin.Latitude, lng = origin.Longitude };
        var destinationObj = new { lat = destination.Lat, lng = destination.Lon };
        await (await moduleTask.Value).InvokeVoidAsync("initRoutesMap", originObj, destinationObj);
    }
    public async ValueTask GenerateLocationsMap(PlacesSearchModel model)
    {
        var module = await moduleTask.Value;
        var json = JsonSerializer.Serialize(model);
        Console.WriteLine($"Locations map data: {json}");
        await module.InvokeVoidAsync("initTaggedMap", json);
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