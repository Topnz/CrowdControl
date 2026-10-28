using CrowdControl.Dashboard.Core.Contracts;
using CrowdControl.Dashboard.Core.Ports;
using Microsoft.JSInterop;

namespace CrowdControl.Dashboard.Services;

// Draws the map (Leaflet via map.js) and renders every snapshot from the heatmap feed.
// Whether the feed is SignalR or a fake is decided in Program.cs.
public class MapService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly IHeatmapFeed _feed;
    private bool _mapInitialized;

    public MapService(IJSRuntime js, IHeatmapFeed feed)
    {
        _js = js;
        _feed = feed;
    }

    public async Task InitializeAsync()
    {
        await _js.InvokeVoidAsync("initMap");
        _mapInitialized = true;

        _feed.SnapshotReceived += RenderAsync;
        await _feed.StartAsync();
    }

    private async Task RenderAsync(HeatmapSnapshotDto snapshot)
    {
        if (_mapInitialized)
        {
            await _js.InvokeVoidAsync("renderHeatmap", snapshot);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _mapInitialized = false;
        _feed.SnapshotReceived -= RenderAsync;
        await _feed.DisposeAsync();
    }
}
