package dk.crowdcontrol.ingestion.adapter.processing;

import dk.crowdcontrol.ingestion.adapter.processing.dto.EventDto;
import dk.crowdcontrol.ingestion.adapter.processing.dto.GeoPointDto;
import dk.crowdcontrol.ingestion.adapter.processing.dto.ZoneDto;
import dk.crowdcontrol.ingestion.domain.Boundary;
import dk.crowdcontrol.ingestion.domain.Event;
import dk.crowdcontrol.ingestion.domain.GeoPoint;
import dk.crowdcontrol.ingestion.domain.Zone;

import java.time.Duration;
import java.util.ArrayList;
import java.util.List;

/**
 * Translates events from GET /events/active (contracts/schemas/shared/common.v1.json)
 * into the domain's Event, Zone and Boundary.
 */
final class EventMapper
{
  private EventMapper()
  {
  }

  /**
   * @param dto the event as received from processing-server
   * @return the event in the domain
   * @throws IllegalArgumentException if the domain rejects the data, e.g. end before start
   * @throws NullPointerException     if a required field is missing
   */
  static Event toDomain(EventDto dto)
  {
    List<Zone> zones = new ArrayList<>();
    for (ZoneDto zoneDto : dto.zones())
    {
      zones.add(toDomain(zoneDto));
    }
    return new Event(
        dto.eventId(),
        dto.name(),
        dto.startTime(),
        dto.endTime(),
        Duration.ofSeconds(dto.sendIntervalSeconds()),
        toBoundary(dto.area()),
        zones);
  }

  private static Boundary toBoundary(List<GeoPointDto> points)
  {
    List<GeoPoint> geoPoints = new ArrayList<>();
    for (GeoPointDto geoPointDto : points)
    {
      geoPoints.add(new GeoPoint(geoPointDto.latitude(), geoPointDto.longitude()));
    }
    return new Boundary(geoPoints);
  }

  private static Zone toDomain(ZoneDto dto)
  {
    return new Zone(dto.zoneId(), dto.name(), toBoundary(dto.area()));
  }
}
