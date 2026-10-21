using System.Collections;

public class Boundary
{
    private List<GeoPoint> _geoPoints;

    public IReadOnlyList<GeoPoint> GeoPoints => _geoPoints.AsReadOnly();

    public void AddGeoPoint(GeoPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        _geoPoints.Add(point);
    }

    private static void ValidateGeoPoints(List<GeoPoint> points)
    {
        if (points.Count < 3)
        {
            throw new ArgumentException(
                "GeoPoints must contain at least 3 points.");
        }

        if (points.Any(p => p == null))
        {
            throw new ArgumentException(
                "GeoPoints cannot contain null points.");
        }
    }

    public void AddGeoPoints(IEnumerable<GeoPoint> points)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        var newPoints = points.ToList();

        ValidateGeoPoints(newPoints);

        _geoPoints = newPoints;
    }
}