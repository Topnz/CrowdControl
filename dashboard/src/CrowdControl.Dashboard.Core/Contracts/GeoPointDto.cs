using System.ComponentModel.DataAnnotations;

namespace CrowdControl.Dashboard.Core.Contracts;

/// <summary>
/// A point on the map in WGS84. Matches GeoPoint in contracts/schemas/shared/common.v1.json.
/// The ranges give a 400 that names the exact point (e.g. "Area[1].Latitude") before the
/// request reaches the domain, which checks the same ranges in its own GeoPoint.
/// </summary>
public class GeoPointDto
{
    [Range(-180.0, 180.0)]
    public required double Longitude { get; set; }

    [Range(-90.0, 90.0)]
    public required double Latitude { get; set; }
}
