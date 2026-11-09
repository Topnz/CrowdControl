namespace CrowdControl.Domain;

/// <summary>
/// An area drawn as a polygon on the map. Always has at least 3 corners; the first corner is
/// not repeated at the end.
/// </summary>
public class Boundary
{
    private List<GeoPoint> _geoPoints = [];

    public IReadOnlyList<GeoPoint> GeoPoints => _geoPoints.AsReadOnly();

    public Boundary(IEnumerable<GeoPoint> points)
    {
        AddGeoPoints(points);
    }

    private void AddGeoPoint(GeoPoint point)
    {
        ArgumentNullException.ThrowIfNull(point);

        _geoPoints.Add(point);
    }

    private void AddGeoPoints(IEnumerable<GeoPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        List<GeoPoint> newPoints = points.ToList();

        ValidateGeoPoints(newPoints);

        _geoPoints = newPoints;
    }

    private static void ValidateGeoPoints(List<GeoPoint> points)
    {
        if (points.Count < 3)
        {
            throw new ArgumentException("GeoPoints must contain at least 3 points.", nameof(points));
        }

        if (points.Any(p => p == null))
        {
            throw new ArgumentException("GeoPoints cannot contain null points.", nameof(points));
        }
    }
}
