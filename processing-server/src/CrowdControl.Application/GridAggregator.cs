using CrowdControl.Shared;

namespace CrowdControl.Application;

public class GridAggregator
{
    private readonly string _eventName;
    private readonly List<Zone> _zones;

    private readonly double _minLat;
    private readonly double _minLon;
    private readonly double _cellSizeLat;
    private readonly double _cellSizeLon;

    private readonly List<CellDto> _cells = new List<CellDto>();
    private readonly List<ZoneCountDto> _zoneCounts = new List<ZoneCountDto>();
    private readonly object _lock = new object();


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
            ZoneCountDto zoneCount = new ZoneCountDto();
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
                CellDto newCell = new CellDto();
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

    public HeatmapSnapshotDto GetSnapshot()
    {
        HeatmapSnapshotDto snapshotDto = new HeatmapSnapshotDto();
        snapshotDto.EventName = _eventName;
        snapshotDto.Timestamp = DateTime.UtcNow;
        snapshotDto.MinLat = _minLat;
        snapshotDto.MinLon = _minLon;
        snapshotDto.CellSizeLat = _cellSizeLat;
        snapshotDto.CellSizeLon = _cellSizeLon;

        lock (_lock)
        {
            for (int i = 0; i < _cells.Count; i++)
            {
                CellDto copy = new CellDto();
                copy.X = _cells[i].X;
                copy.Y = _cells[i].Y;
                copy.Count = _cells[i].Count;
                snapshotDto.Cells.Add(copy);
            }

            for (int i = 0; i < _zoneCounts.Count; i++)
            {
                ZoneCountDto copy = new ZoneCountDto();
                copy.Name = _zoneCounts[i].Name;
                copy.Capacity = _zoneCounts[i].Capacity;
                copy.Count = _zoneCounts[i].Count;
                snapshotDto.Zones.Add(copy);
            }
        }

        return snapshotDto;
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
