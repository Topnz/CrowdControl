namespace CrowdControl.Application.MonitorEvent;

/// <summary>
/// Point-in-time view of the heatmap for an event, produced by <see cref="GridAggregator"/>.
/// Independent of how it is sent; the Api maps it to its own DTO.
/// </summary>
public class HeatmapSnapshot
{
    public string EventName { get; set; } = "";
    public DateTime Timestamp { get; set; }

    // Grid origin (south-west corner of cell 0, 0) and cell size in degrees
    public double MinLat { get; set; }
    public double MinLon { get; set; }
    public double CellSizeLat { get; set; }
    public double CellSizeLon { get; set; }

    public List<GridCell> Cells { get; set; } = new List<GridCell>();
    public List<ZoneCount> Zones { get; set; } = new List<ZoneCount>();
}
