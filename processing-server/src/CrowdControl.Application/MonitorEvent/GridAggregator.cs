namespace CrowdControl.Application.MonitorEvent;

public class GridAggregator
{
    private readonly string _eventName;
    private readonly List<Zone> _zones;

    private readonly double _minLat;
    private readonly double _minLon;
    private readonly double _cellSizeLat;
    private readonly double _cellSizeLon;

    private readonly List<GridCell> _cells = new List<GridCell>();
    private readonly List<ZoneCount> _zoneCounts = new List<ZoneCount>();
    private readonly object _lock = new object();


    // Uden event: grid'et starter i (0, 0), så alle positioner fra køen kan placeres, og der er ingen zoner
    public GridAggregator(string eventName, double cellSizeLat = 0.0002, double cellSizeLon = 0.00035)
    {
        _eventName = eventName;
        _cellSizeLat = cellSizeLat;
        _cellSizeLon = cellSizeLon;
        _zones = new List<Zone>();
        _minLat = 0;
        _minLon = 0;
    }

    //Lat = 0.0002 og Lon = 0.00035 laver kradrater på cirka 22 meter ifølge clankeren, ved ikke om det reelt er de rigtige tal.
    public GridAggregator(Event ev, double cellSizeLat = 0.0002, double cellSizeLon = 0.00035)
    {
        _eventName = ev.Name;
        _cellSizeLat = cellSizeLat;
        _cellSizeLon = cellSizeLon;

        if (ev.Zones == null)
        {
            _zones = new List<Zone>();
        }
        else
        {
            _zones = ev.Zones;
        }

        // grid bliver bygget fra laveste venstre hjørne 
        _minLat = double.MaxValue;
        _minLon = double.MaxValue;
        IReadOnlyList<GeoPoint> corners = ev.Perimiter.GeoPoints;
        for (int i = 0; i < corners.Count; i++)
        {
            if (corners[i].Latitude < _minLat)
            {
                _minLat = corners[i].Latitude;
            }
            if (corners[i].Longitude < _minLon)
            {
                _minLon = corners[i].Longitude;
            }
        }

        for (int i = 0; i < _zones.Count; i++)
        {
            ZoneCount zoneCount = new ZoneCount();
            zoneCount.Name = _zones[i].Name;
            zoneCount.Capacity = _zones[i].Capacity;
            zoneCount.Count = 0;
            _zoneCounts.Add(zoneCount);
        }
    }

    public void Add(double latitude, double longitude)
    {
        int x = (int)Math.Floor((longitude - _minLon) / _cellSizeLon);
        int y = (int)Math.Floor((latitude - _minLat) / _cellSizeLat);

        GeoPoint point = new GeoPoint { Latitude = latitude, Longitude = longitude };

        lock (_lock)
        {
            bool cellFound = false;
            for (int i = 0; i < _cells.Count; i++)
            {
                if (_cells[i].X == x && _cells[i].Y == y)
                {
                    _cells[i].Count++;
                    cellFound = true;
                    break;
                }
            }

            if (cellFound == false)
            {
                GridCell newCell = new GridCell();
                newCell.X = x;
                newCell.Y = y;
                newCell.Count = 1;
                _cells.Add(newCell);
            }

            for (int i = 0; i < _zones.Count; i++)
            {
                Zone zone = _zones[i];
                if (zone.Boundary != null)
                {
                    if (PolygonHelper.IsInside(zone.Boundary.GeoPoints, point))
                    {
                        _zoneCounts[i].Count++;
                    }
                }
            }
        }
    }

    public HeatmapSnapshot GetSnapshot()
    {
        HeatmapSnapshot snapshot = new HeatmapSnapshot();
        snapshot.EventName = _eventName;
        snapshot.Timestamp = DateTime.UtcNow;
        snapshot.MinLat = _minLat;
        snapshot.MinLon = _minLon;
        snapshot.CellSizeLat = _cellSizeLat;
        snapshot.CellSizeLon = _cellSizeLon;

        lock (_lock)
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                GridCell copy = new GridCell();
                copy.X = _cells[i].X;
                copy.Y = _cells[i].Y;
                copy.Count = _cells[i].Count;
                snapshot.Cells.Add(copy);
            }

            for (int i = 0; i < _zoneCounts.Count; i++)
            {
                ZoneCount copy = new ZoneCount();
                copy.Name = _zoneCounts[i].Name;
                copy.Capacity = _zoneCounts[i].Capacity;
                copy.Count = _zoneCounts[i].Count;
                snapshot.Zones.Add(copy);
            }
        }

        return snapshot;
    }

    public void Reset()
    {
        lock (_lock)
        {
            _cells.Clear();

            for (int i = 0; i < _zoneCounts.Count; i++)
            {
                _zoneCounts[i].Count = 0;
            }
        }
    }
}
