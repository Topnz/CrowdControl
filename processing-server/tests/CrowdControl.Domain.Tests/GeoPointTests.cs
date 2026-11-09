namespace CrowdControl.Domain.Tests;

/// <summary>
/// A GeoPoint only accepts coordinates that exist on the map (WGS84).
/// </summary>
public class GeoPointTests
{
    [Theory]
    [InlineData(55.86, 9.85)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(0, 0)]
    public void Create_ValidCoordinates_KeepsValues(double latitude, double longitude)
    {
        GeoPoint point = new GeoPoint { Latitude = latitude, Longitude = longitude };

        Assert.Equal(latitude, point.Latitude);
        Assert.Equal(longitude, point.Longitude);
    }

    [Theory]
    [InlineData(90.0001)]
    [InlineData(-90.0001)]
    [InlineData(500)]
    [InlineData(double.NaN)]
    public void Create_LatitudeOutOfRange_Throws(double latitude)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GeoPoint { Latitude = latitude, Longitude = 9.85 });

        Assert.Equal("Latitude", exception.ParamName);
    }

    [Theory]
    [InlineData(180.0001)]
    [InlineData(-180.0001)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NaN)]
    public void Create_LongitudeOutOfRange_Throws(double longitude)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GeoPoint { Latitude = 55.86, Longitude = longitude });

        Assert.Equal("Longitude", exception.ParamName);
    }
}
