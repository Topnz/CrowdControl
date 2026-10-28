package dk.crowdcontrol.ingestion.domain;

import java.awt.geom.Path2D;
import java.util.List;

/**
 * An area drawn as a polygon on the map (the event area or a zone).
 */
public class Boundary
{
  private final List<GeoPoint> geoPoints;
  private final Path2D.Double polygon;

  /**
   * @param geoPoints the polygon's corners in the order they were drawn. The polygon is
   *                  closed automatically between the last and the first corner.
   * @throws IllegalArgumentException if there are fewer than 3 corners
   */
  public Boundary(List<GeoPoint> geoPoints)
  {
    if (geoPoints == null || geoPoints.size() < 3)
      throw new IllegalArgumentException("En polygon skal have mindst 3 hjørner");
    this.geoPoints = List.copyOf(geoPoints);
    this.polygon = toPolygon(this.geoPoints);
  }

  /** @return the polygon's corners as an unmodifiable list */
  public List<GeoPoint> getGeoPoints()
  {
    return geoPoints;
  }

  /**
   * Determines whether the point lies inside the polygon.
   * Treats lat/lon as planar coordinates (x = longitude, y = latitude), which
   * is accurate enough for areas the size of a festival.
   *
   * @param point the point to check
   * @return true if the point lies inside the polygon
   */
  public boolean contains(GeoPoint point)
  {
    return polygon.contains(point.getLongitude(), point.getLatitude());
  }

  /**
   * Draws the polygon: start at the first corner, draw a line to each of the next ones, and close it.
   *
   * @param geoPoints the corners (at least 3)
   * @return the polygon with x = longitude and y = latitude
   */
  private static Path2D.Double toPolygon(List<GeoPoint> geoPoints)
  {
    Path2D.Double polygon = new Path2D.Double();
    GeoPoint first = geoPoints.getFirst();

    polygon.moveTo(first.getLongitude(), first.getLatitude());

    // Start at index 1, since the first corner is already set with moveTo
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
