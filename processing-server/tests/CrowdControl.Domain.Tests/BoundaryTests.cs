namespace CrowdControl.Domain.Tests;

/// <summary>
/// A Boundary is a polygon with at least 3 corners, validated on construction.
/// </summary>
public class BoundaryTests
{
    [Fact]
    public void Constructor_ThreePoints_KeepsPointsInOrder()
    {
        List<GeoPoint> points = Points(3);

        Boundary boundary = new Boundary(points);

        Assert.Equal(points, boundary.GeoPoints);
    }

    [Fact]
    public void Constructor_TwoPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Boundary(Points(2)));
    }

    [Fact]
    public void Constructor_NullPoint_Throws()
    {
        List<GeoPoint> points = Points(3);
        points[1] = null!;

        Assert.Throws<ArgumentException>(() => new Boundary(points));
    }

    [Fact]
    public void Constructor_NoPoints_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Boundary(null!));
    }

    // The boundary keeps its own copy, so changing the caller's list afterwards does not change it
    [Fact]
    public void Constructor_CallerChangesListAfterwards_BoundaryIsUnchanged()
    {
        List<GeoPoint> points = Points(3);
        Boundary boundary = new Boundary(points);

        points.Clear();

        Assert.Equal(3, boundary.GeoPoints.Count);
    }

    private static List<GeoPoint> Points(int count)
    {
        List<GeoPoint> points = new List<GeoPoint>();
        for (int i = 0; i < count; i++)
        {
            points.Add(new GeoPoint { Latitude = 55.86 + i * 0.001, Longitude = 9.85 + (i % 2) * 0.001 });
        }
        return points;
    }
}
