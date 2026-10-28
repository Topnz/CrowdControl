namespace CrowdControl.Dashboard.Contracts;

/// <summary>
/// Heatmap snapshot received from processing-server via SignalR (HeatmapUpdated) and GET /heatmap.
/// Written by hand against contracts/schemas/signalr/HeatmapSnapshot.v1.json; there is
/// deliberately no project reference to processing-server.
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
