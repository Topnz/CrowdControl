namespace CrowdControl.Application.MonitorEvent;

/// <summary>
/// Number of positions in one grid cell. X counts eastwards and Y northwards from the grid origin.
/// </summary>
public class GridCell
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Count { get; set; }
}
