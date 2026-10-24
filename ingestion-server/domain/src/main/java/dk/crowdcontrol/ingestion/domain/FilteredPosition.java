package dk.crowdcontrol.ingestion.domain;

import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.UUID;

/**
 * En position, der er godkendt til et aktivt event, sammen med de zoner, den
 * ligger i. Det er det, der sendes videre til processing-server.
 * Listen af zoner er tom, hvis positionen ligger i eventet, men uden for alle zoner.
 */
public class FilteredPosition
{
  private final Position position;
  private final UUID eventId;
  private final List<UUID> zoneIds;

  public FilteredPosition(Position position, UUID eventId, List<UUID> zoneIds)
  {
    this.position = Objects.requireNonNull(position, "position cannot be null");
    this.eventId = Objects.requireNonNull(eventId, "eventId cannot be null");
    this.zoneIds = List.copyOf(zoneIds);
  }

  /** Tom, hvis positionen ligger uden for eventets område eller tidsrum. */
  public static Optional<FilteredPosition> from(Event event, Position position)
  {
    if (!event.accepts(position))
      return Optional.empty();
    List<UUID> zoneIds = event.zonesContaining(position.getGeoPoint()).stream()
        .map(Zone::getId)
        .toList();
    return Optional.of(new FilteredPosition(position, event.getId(), zoneIds));
  }

  public Position getPosition()
  {
    return position;
  }

  public UUID getEventId()
  {
    return eventId;
  }

  public List<UUID> getZoneIds()
  {
    return zoneIds;
  }
}
