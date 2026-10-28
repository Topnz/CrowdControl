namespace CrowdControl.Api.Contracts;

/// <summary>
/// Heatmap snapshot as sent to the dashboard via SignalR (HeatmapUpdated) and GET /heatmap.
/// Must match contracts/schemas/signalr/HeatmapSnapshot.v1.json.
/// </summary>
public class HeatmapSnapshotDto
{
    public string EventName { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public double MinLat { get; set; }
    public double MinLon { get; set; }
    public double CellSizeLat { get; set; }
    public double CellSizeLon { get; set; }
    public List<CellDto> Cells { get; set; } = new List<CellDto>();
    public List<ZoneCountDto> Zones { get; set; } = new List<ZoneCountDto>();
}
