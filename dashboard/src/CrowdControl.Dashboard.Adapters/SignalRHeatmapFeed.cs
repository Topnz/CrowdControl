using CrowdControl.Dashboard.Core.Contracts;
using CrowdControl.Dashboard.Core.Ports;
using Microsoft.AspNetCore.SignalR.Client;

namespace CrowdControl.Dashboard.Adapters;

/// <summary>
/// Receives snapshots from HeatmapBroadcaster in the Api (/hubs/heatmap), which sends one every
/// 3 seconds. See contracts/asyncapi/dashboard.yaml.
/// </summary>
public class SignalRHeatmapFeed : IHeatmapFeed
{
    private readonly ApiAddress _api;
    private HubConnection? _hub;

    public event Func<HeatmapSnapshotDto, Task>? SnapshotReceived;

    public SignalRHeatmapFeed(ApiAddress api)
    {
        _api = api;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _hub = new HubConnectionBuilder()
            .WithUrl(_api.Resolve("/hubs/heatmap"))
            .WithAutomaticReconnect()
            .Build();

        _hub.On<HeatmapSnapshotDto>("HeatmapUpdated", async snapshot =>
        {
            Func<HeatmapSnapshotDto, Task>? handler = SnapshotReceived;
            if (handler != null)
            {
                await handler(snapshot);
            }
        });

        await _hub.StartAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }
    }
}
