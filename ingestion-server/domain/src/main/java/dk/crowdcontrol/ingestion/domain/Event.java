package dk.crowdcontrol.ingestion.domain;

import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

/**
 * Copy of an event from processing-server: time span, area, send interval and zones.
 * Used to decide whether a position is accepted and which zones it lies in.
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

  /**
   * @param id           the event's ID from processing-server
   * @param name         the event's name
   * @param startTime    start time (inclusive)
   * @param endTime      end time (exclusive); must be after startTime
   * @param sendInterval how often the mobile app should send the position (ID-29)
   * @param perimeter    the event's area
   * @param zones        the event's zones; copied to an unmodifiable list
   * @throws IllegalArgumentException if endTime is not after startTime
   * @throws NullPointerException     if id, startTime, endTime, sendInterval, perimeter
   *                                  or zones is null
   */
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

  /** @return the event's ID */
  public UUID getId()
  {
    return id;
  }

  /** @return the event's name */
  public String getName()
  {
    return name;
  }

  /** @return the start time (inclusive) */
  public Instant getStartTime()
  {
    return startTime;
  }

  /** @return the end time (exclusive) */
  public Instant getEndTime()
  {
    return endTime;
  }

  /** @return how often the mobile app should send the position */
  public Duration getSendInterval()
  {
    return sendInterval;
  }

  /** @return the event's area */
  public Boundary getPerimeter()
  {
    return perimeter;
  }

  /** @return the event's zones as an unmodifiable list */
  public List<Zone> getZones()
  {
    return zones;
  }

  /**
   * @param time the time to check
   * @return true if the time is at or after the start time and before the end time
   */
  public boolean isActiveAt(Instant time)
  {
    return !time.isBefore(startTime) && time.isBefore(endTime);
  }

  /**
   * @param position the position to check
   * @return true if the position lies within the event's area and time span
   */
  public boolean accepts(Position position)
  {
    return isActiveAt(position.getTimestamp()) && perimeter.contains(position.getGeoPoint());
  }

  /**
   * Finds the zones the point lies in. Zones can overlap, so there can be several.
   *
   * @param point the point to check
   * @return the zones containing the point; empty if it lies outside all zones
   */
  public List<Zone> zonesContaining(GeoPoint point)
  {
    return zones.stream().filter(zone -> zone.contains(point)).toList();
  }
}
