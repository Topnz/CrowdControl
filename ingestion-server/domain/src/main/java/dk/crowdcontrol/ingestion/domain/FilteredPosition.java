package dk.crowdcontrol.ingestion.domain;

import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.UUID;

/**
 * A position accepted for an active event, together with the zones it
 * lies in. This is what is forwarded to processing-server.
 * The list of zones is empty if the position is inside the event but outside all zones.
 */
public class FilteredPosition
{
  private final Position position;
  private final UUID eventId;
  private final List<UUID> zoneIds;

  /**
   * Creates an accepted position directly. Normally use {@link #from(Event, Position)},
   * which also checks area and time span.
   *
   * @param position the accepted position
   * @param eventId  the event the position belongs to
   * @param zoneIds  the zones the position lies in; copied to an unmodifiable list
   * @throws NullPointerException if position, eventId or zoneIds is null
   */
  public FilteredPosition(Position position, UUID eventId, List<UUID> zoneIds)
  {
    this.position = Objects.requireNonNull(position, "position cannot be null");
    this.eventId = Objects.requireNonNull(eventId, "eventId cannot be null");
    this.zoneIds = List.copyOf(zoneIds);
  }

  /**
   * Filters a position against an event and finds the zones it lies in.
   *
   * @param event    the event the position was sent to
   * @param position the position from the mobile app
   * @return the accepted position, or empty if it lies outside the event's area or time span
   */
  public static Optional<FilteredPosition> from(Event event, Position position)
  {
    if (!event.accepts(position))
      return Optional.empty();
    List<UUID> zoneIds = event.zonesContaining(position.getGeoPoint()).stream()
        .map(Zone::getId)
        .toList();
    return Optional.of(new FilteredPosition(position, event.getId(), zoneIds));
  }

  /** @return the accepted position */
  public Position getPosition()
  {
    return position;
  }

  /** @return the ID of the event the position belongs to */
  public UUID getEventId()
  {
    return eventId;
  }

  /** @return the IDs of the zones the position lies in; empty if it lies outside all zones */
  public List<UUID> getZoneIds()
  {
    return zoneIds;
  }
}
