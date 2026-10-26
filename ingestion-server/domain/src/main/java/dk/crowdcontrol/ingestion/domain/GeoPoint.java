package dk.crowdcontrol.ingestion.domain;

public class GeoPoint
{
  private final double latitude;
  private final double longitude;

  public GeoPoint(double latitude, double longitude)
  {
    if (latitude < -90 || latitude > 90)
      throw new IllegalArgumentException("latitude skal ligge mellem -90 og 90");
    if (longitude < -180 || longitude > 180)
      throw new IllegalArgumentException("longitude skal ligge mellem -180 og 180");
    this.latitude = latitude;
    this.longitude = longitude;
  }

  public double getLatitude()
  {
    return latitude;
  }

  public double getLongitude()
  {
    return longitude;
  }
}
