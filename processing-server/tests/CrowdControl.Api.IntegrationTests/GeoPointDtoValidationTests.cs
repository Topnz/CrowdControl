using System.ComponentModel.DataAnnotations;
using CrowdControl.Api.Contracts;

namespace CrowdControl.Api.IntegrationTests;

/// <summary>
/// Checks the [Range] attributes on GeoPointDto, which [ApiController] turns into a 400 before
/// the request reaches the domain. The ranges match GeoPoint in common.v1.json.
/// </summary>
public class GeoPointDtoValidationTests
{
    [Theory]
    [InlineData(55.86, 9.85)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void ValidCoordinates_HaveNoErrors(double latitude, double longitude)
    {
        List<ValidationResult> errors = Validate(new GeoPointDto { Latitude = latitude, Longitude = longitude });

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(90.0001)]
    [InlineData(-90.0001)]
    [InlineData(500)]
    public void LatitudeOutOfRange_GivesErrorOnLatitude(double latitude)
    {
        List<ValidationResult> errors = Validate(new GeoPointDto { Latitude = latitude, Longitude = 9.85 });

        ValidationResult error = Assert.Single(errors);
        Assert.Contains(nameof(GeoPointDto.Latitude), error.MemberNames);
    }

    [Theory]
    [InlineData(180.0001)]
    [InlineData(-180.0001)]
    public void LongitudeOutOfRange_GivesErrorOnLongitude(double longitude)
    {
        List<ValidationResult> errors = Validate(new GeoPointDto { Latitude = 55.86, Longitude = longitude });

        ValidationResult error = Assert.Single(errors);
        Assert.Contains(nameof(GeoPointDto.Longitude), error.MemberNames);
    }

    private static List<ValidationResult> Validate(GeoPointDto dto)
    {
        List<ValidationResult> errors = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), errors, validateAllProperties: true);
        return errors;
    }
}
