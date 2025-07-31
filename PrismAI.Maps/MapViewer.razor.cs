using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PrismAI.Core.Models;
using Location = PrismAI.Core.Models.ResponseModels.Location;

namespace PrismAI.Maps;
public partial class MapViewer
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Parameter]
    public Location? Destination { get; set; }

    private double _prevLat;
    private double _prevLon;
    private PrismMapJsInterop HeatMapInterop => new(JsRuntime);
    protected override async Task OnParametersSetAsync()
    {
        
        if (Destination?.Lat is not null && Destination?.Lon is not null && Math.Abs(Destination.Lat - _prevLat) > 0.001 && Math.Abs(Destination.Lon - _prevLon) > 0.001)
        {
            _prevLat = Destination.Lat;
            _prevLon = Destination.Lon;
            await HeatMapInterop.GenerateRoutesMap(Destination);
        }
        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // This is where you can call JS functions after the component has rendered
            // For example, you might want to initialize a map or perform some other JS interop
            if (Destination?.Lat is not null && Destination?.Lon is not null && Math.Abs(Destination.Lat - _prevLat) > 0.001 && Math.Abs(Destination.Lon - _prevLon) > 0.001)
            {
                _prevLat = Destination.Lat;
                _prevLon = Destination.Lon;
                await HeatMapInterop.GenerateRoutesMap(Destination);
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
