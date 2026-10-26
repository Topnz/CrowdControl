
using Microsoft.JSInterop;

namespace CrowdControl.Dashboard.Services;

public class MapService
{
    private readonly IJSRuntime _js;
    private bool _mapInitialized;

    public MapService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        await _js.InvokeVoidAsync("initMap");
        _mapInitialized = true;

        await AddPointAsync(37.50024109655184, -122.28528451834352);
    }

    public async Task AddPointAsync(
        double latitude,
        double longitude)
    {
        if (!_mapInitialized)
        {
            throw new InvalidOperationException(
                "The map has not been initialized.");
        }

        await _js.InvokeVoidAsync(
            "addMapPoint",
            latitude,
            longitude);
    }
}
