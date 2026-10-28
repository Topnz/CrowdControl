using CrowdControl.Application.MonitorEvent;

namespace CrowdControl.Api.Contracts;

/// <summary>
/// Maps the Application model to the DTO that is sent to the dashboard.
/// </summary>
public static class HeatmapSnapshotMapping
{
    public static HeatmapSnapshotDto ToDto(this HeatmapSnapshot snapshot)
    {
        HeatmapSnapshotDto dto = new HeatmapSnapshotDto();
        dto.EventName = snapshot.EventName;
        dto.Timestamp = snapshot.Timestamp;
        dto.MinLat = snapshot.MinLat;
        dto.MinLon = snapshot.MinLon;
        dto.CellSizeLat = snapshot.CellSizeLat;
        dto.CellSizeLon = snapshot.CellSizeLon;

        foreach (GridCell cell in snapshot.Cells)
        {
            dto.Cells.Add(new CellDto { X = cell.X, Y = cell.Y, Count = cell.Count });
        }

        foreach (ZoneCount zone in snapshot.Zones)
        {
            dto.Zones.Add(new ZoneCountDto { Name = zone.Name, Capacity = zone.Capacity, Count = zone.Count });
        }

        return dto;
    }
}
