package dk.crowdcontrol.ingestion.domain;

import java.util.Objects;
import java.util.UUID;

/**
 * Kopi af en zone fra processing-server. Java bruger den kun til at afgøre,
 * hvilke zoner en position ligger i; kapacitet og optælling håndteres i C#.
 */
public class Zone
{
  private final UUID id;
  private final String name;
  private final Boundary boundary;

  public Zone(UUID id, String name, Boundary boundary)
  {
    this.id = Objects.requireNonNull(id, "id cannot be null");
    this.name = name;
    this.boundary = Objects.requireNonNull(boundary, "boundary cannot be null");
  }

  public UUID getId()
  {
    return id;
  }

  public String getName()
  {
    return name;
  }

  public Boundary getBoundary()
  {
    return boundary;
  }

  public boolean contains(GeoPoint point)
  {
    return boundary.contains(point);
  }
}
