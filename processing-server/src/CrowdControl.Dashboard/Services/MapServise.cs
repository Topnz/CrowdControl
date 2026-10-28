using CrowdControl.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace CrowdControl.Dashboard.Services;

public class MapService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly ApiAddress _api;
    private HubConnection? _hub;
    private bool _mapInitialized;

    public MapService(IJSRuntime js, ApiAddress api)
    {
        _js = js;
        _api = api;
    }

    public async Task InitializeAsync()
    {
        await _js.InvokeVoidAsync("initMap");
        _mapInitialized = true;

        await StartHeatmapFeedAsync();
    }

    // Lytter på HeatmapBroadcaster i Api (/hubs/heatmap), som sender et snapshot hvert 3. sekund
    private async Task StartHeatmapFeedAsync()
    {
        _hub = new HubConnectionBuilder()
            .WithUrl(_api.Resolve("/hubs/heatmap"))
            .WithAutomaticReconnect()
            .Build();

        _hub.On<HeatmapSnapshotDto>("HeatmapUpdated", async snapshot =>
        {
            if (_mapInitialized)
            {
                await _js.InvokeVoidAsync("renderHeatmap", snapshot);
            }
        });

        await _hub.StartAsync();
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

    public async ValueTask DisposeAsync()
    {
        _mapInitialized = false;
        if (_hub != null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }
    }
}
