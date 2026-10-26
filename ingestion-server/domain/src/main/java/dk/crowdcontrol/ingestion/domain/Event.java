package dk.crowdcontrol.ingestion.domain;

import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

/**
 * Kopi af et event fra processing-server: tidsrum, område, sendeinterval og zoner.
 */
public class Event
{
  private final UUID id;
  private final String name;
  private final Instant startTime;
  private final Instant endTime;
  private final Duration sendInterval;
  private final Boundary perimeter;
  private final List<Zone> zones;

  public Event(UUID id, String name, Instant startTime, Instant endTime,
               Duration sendInterval, Boundary perimeter, List<Zone> zones)
  {
    if (!endTime.isAfter(startTime))
      throw new IllegalArgumentException("endTime skal ligge efter startTime");
    this.id = Objects.requireNonNull(id, "id");
    this.name = name;
    this.startTime = startTime;
    this.endTime = endTime;
    this.sendInterval = Objects.requireNonNull(sendInterval, "sendInterval");
    this.perimeter = Objects.requireNonNull(perimeter, "perimeter");
    this.zones = List.copyOf(zones);
  }

  public UUID getId()
  {
    return id;
  }

  public String getName()
  {
    return name;
  }

  public Instant getStartTime()
  {
    return startTime;
  }

  public Instant getEndTime()
  {
    return endTime;
  }

  public Duration getSendInterval()
  {
    return sendInterval;
  }

  public Boundary getPerimeter()
  {
    return perimeter;
  }

  public List<Zone> getZones()
  {
    return zones;
  }

  public boolean isActiveAt(Instant time)
  {
    return !time.isBefore(startTime) && time.isBefore(endTime);
  }

  /** Sand hvis positionen ligger inden for eventets område og tidsrum. */
  public boolean accepts(Position position)
  {
    return isActiveAt(position.getTimestamp()) && perimeter.contains(position.getGeoPoint());
  }

  /** De zoner, punktet ligger i. Zoner kan overlappe, så der kan være flere. */
  public List<Zone> zonesContaining(GeoPoint point)
  {
    return zones.stream().filter(zone -> zone.contains(point)).toList();
  }
}
