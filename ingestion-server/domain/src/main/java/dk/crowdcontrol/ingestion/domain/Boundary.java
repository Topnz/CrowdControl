package dk.crowdcontrol.ingestion.domain;

import java.awt.geom.Path2D;
import java.util.List;

/**
 * Et område tegnet som polygon på kortet (eventets område eller en zone).
 */
public class Boundary
{
  private final List<GeoPoint> geoPoints;
  private final Path2D.Double polygon;

  public Boundary(List<GeoPoint> geoPoints)
  {
    if (geoPoints == null || geoPoints.size() < 3)
      throw new IllegalArgumentException("En polygon skal have mindst 3 hjørner");
    this.geoPoints = List.copyOf(geoPoints);
    this.polygon = toPolygon(this.geoPoints);
  }

  public List<GeoPoint> getGeoPoints()
  {
    return geoPoints;
  }

  /**
   * Bruger lat/lon som plane koordinater (x = longitude, y = latitude), hvilket
   * er præcist nok for områder på størrelse med en festival.
   */
  public boolean contains(GeoPoint point)
  {
    return polygon.contains(point.getLongitude(), point.getLatitude());
  }

  /** Tegner polygonen: start i første hjørne, træk en linje til hvert af de næste, og luk den. */
  private static Path2D.Double toPolygon(List<GeoPoint> geoPoints)
  {
    Path2D.Double polygon = new Path2D.Double();
    GeoPoint first = geoPoints.getFirst();

    polygon.moveTo(first.getLongitude(), first.getLatitude());

    // Start ved index 1, da første hjørne allerede er sat med moveTo
    for (int i = 1; i < geoPoints.size(); i++)
    {
      GeoPoint corner = geoPoints.get(i);
      double x = corner.getLongitude();
      double y = corner.getLatitude();
      polygon.lineTo(x, y);
    }

    polygon.closePath();
    return polygon;
  }
}
