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
    private DotNetObjectReference<MapService>? _dotNetReference;
    public event Action<List<GeoPointDto>>? PolygonCompleted;

    public MapService(IJSRuntime js, IHeatmapFeed feed)
    {
        _js = js;
        _feed = feed;
    }

    public async Task InitializeAsync()
    {
        _dotNetReference = DotNetObjectReference.Create(this);

        await _js.InvokeVoidAsync("initMap", _dotNetReference);
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
        // Everything that belongs to this page is removed before the first await: Blazor disposes
        // the old page and then runs the new page's OnAfterRenderAsync without waiting for this
        // method, so anything done after an await would hit the new page's map and reference.
        _mapInitialized = false;
        _feed.SnapshotReceived -= RenderAsync;
        DotNetObjectReference<MapService>? dotNetReference = _dotNetReference;
        _dotNetReference = null;
        await _js.InvokeVoidAsync("disposeMap");
        dotNetReference?.Dispose();
        await _feed.DisposeAsync();
    }

    [JSInvokable]
    public Task OnPolygonCompleted(List<GeoPointDto> coordinates)
    {
        PolygonCompleted?.Invoke(coordinates);

        return Task.CompletedTask;
    }

    public Task StartPolygonDrawingAsync()
    {
        return _js.InvokeVoidAsync(
            "startPolygonDrawing"
        ).AsTask();
    }

    public async Task FinishPolygonDrawingAsync()
    {
        await _js.InvokeVoidAsync("finishPolygonDrawing");
    }

    public Task CancelPolygonDrawingAsync()
    {
        return _js.InvokeVoidAsync(
            "cancelPolygonDrawing"
        ).AsTask();
    }
}
