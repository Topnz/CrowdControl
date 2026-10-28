package dk.crowdcontrol.ingestion.domain;

/**
 * A point on the map given by latitude and longitude in degrees (WGS 84,
 * as used by the Geolocation API).
 */
public class GeoPoint
{
  private final double latitude;
  private final double longitude;

  /**
   * @param latitude  latitude between -90 and 90
   * @param longitude longitude between -180 and 180
   * @throws IllegalArgumentException if either value is outside its range
   */
  public GeoPoint(double latitude, double longitude)
  {
    if (latitude < -90 || latitude > 90)
      throw new IllegalArgumentException("latitude skal ligge mellem -90 og 90");
    if (longitude < -180 || longitude > 180)
      throw new IllegalArgumentException("longitude skal ligge mellem -180 og 180");
    this.latitude = latitude;
    this.longitude = longitude;
  }

  /** @return the latitude in degrees */
  public double getLatitude()
  {
    return latitude;
  }

  /** @return the longitude in degrees */
  public double getLongitude()
  {
    return longitude;
  }
}
