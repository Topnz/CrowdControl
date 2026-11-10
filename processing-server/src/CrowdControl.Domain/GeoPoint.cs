namespace CrowdControl.Domain;

/// <summary>
/// A point on the map in WGS84 (the same as the Geolocation API). Latitude is between -90 and 90,
/// longitude between -180 and 180, as in GeoPoint in contracts/schemas/shared/common.v1.json.
/// A point never changes after it is created (init only).
/// </summary>
public class GeoPoint
{
    private readonly double _latitude;
    private readonly double _longitude;

    public required double Latitude
    {
        get => _latitude;
        init
        {
            ThrowIfOutOfRange(value, 90, nameof(Latitude));
            _latitude = value;
        }
    }

    public required double Longitude
    {
        get => _longitude;
        init
        {
            ThrowIfOutOfRange(value, 180, nameof(Longitude));
            _longitude = value;
        }
    }

    // NaN fails both comparisons, so it is rejected as well
    private static void ThrowIfOutOfRange(double value, double limit, string paramName)
    {
        if (!(value >= -limit && value <= limit))
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be between -{limit} and {limit}.");
        }
    }
}
