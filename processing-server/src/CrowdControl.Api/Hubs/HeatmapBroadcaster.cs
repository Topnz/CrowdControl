using CrowdControl.Api.Contracts;
using CrowdControl.Application.MonitorEvent;
using Microsoft.AspNetCore.SignalR;

namespace CrowdControl.Api.Hubs;

public class HeatmapBroadcaster : BackgroundService
{
    private readonly GridAggregator _aggregator;
    private readonly IHubContext<HeatmapHub> _hub;
    private readonly LatestHeatmap _latest;

    public HeatmapBroadcaster(GridAggregator aggregator, IHubContext<HeatmapHub> hub, LatestHeatmap latest)
    {
        _aggregator = aggregator;
        _hub = hub;
        _latest = latest;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            //har sat den til 3 sekunder, fordi det er jeg talte da jeg prøvede P og T's cloudflare link
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            HeatmapSnapshotDto snapshotDto = _aggregator.GetSnapshot().ToDto();
            // Slået fra: cellerne lægges sammen over tid i stedet for at nulstilles hvert 3. sekund
            // _aggregator.Reset();
            _latest.Snapshot = snapshotDto;

            await _hub.Clients.All.SendAsync("HeatmapUpdated", snapshotDto, stoppingToken);
        }
    }
}
