package dk.crowdcontrol.ingestion.domain;

import java.util.Objects;
import java.util.UUID;

/**
 * Copy of a zone from processing-server. Java only uses it to determine
 * which zones a position lies in; capacity and counting are handled in C#.
 */
public class Zone
{
  private final UUID id;
  private final String name;
  private final Boundary boundary;

  /**
   * @param id       the zone's ID from processing-server
   * @param name     the zone's name, e.g. "Bøgescenen Sektion A"
   * @param boundary the zone's area
   * @throws NullPointerException if id or boundary is null
   */
  public Zone(UUID id, String name, Boundary boundary)
  {
    this.id = Objects.requireNonNull(id, "id cannot be null");
    this.name = name;
    this.boundary = Objects.requireNonNull(boundary, "boundary cannot be null");
  }

  /** @return the zone's ID */
  public UUID getId()
  {
    return id;
  }

  /** @return the zone's name */
  public String getName()
  {
    return name;
  }

  /** @return the zone's area */
  public Boundary getBoundary()
  {
    return boundary;
  }

  /**
   * @param point the point to check
   * @return true if the point lies inside the zone
   */
  public boolean contains(GeoPoint point)
  {
    return boundary.contains(point);
  }
}
