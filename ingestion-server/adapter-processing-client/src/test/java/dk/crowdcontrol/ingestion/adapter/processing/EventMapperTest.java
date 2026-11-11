package dk.crowdcontrol.ingestion.adapter.processing;

import dk.crowdcontrol.ingestion.adapter.processing.dto.EventDto;
import dk.crowdcontrol.ingestion.adapter.processing.dto.GeoPointDto;
import dk.crowdcontrol.ingestion.adapter.processing.dto.ZoneDto;
import dk.crowdcontrol.ingestion.domain.Event;
import dk.crowdcontrol.ingestion.domain.GeoPoint;
import dk.crowdcontrol.ingestion.domain.Zone;
import org.junit.jupiter.api.Test;

import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Tests {@link EventMapper}: that every field in the contract ends up in the domain,
 * and that invalid data is rejected by the domain.
 */
class EventMapperTest
{
  private static final Instant START = Instant.parse("2026-07-01T10:00:00Z");
  private static final Instant END = Instant.parse("2026-07-01T22:00:00Z");

  @Test
  void allFieldsAreMapped()
  {
    UUID eventId = UUID.randomUUID();
    UUID zoneId = UUID.randomUUID();
    EventDto dto = new EventDto(eventId, "Roskilde", START, END, 30, square(0, 0, 4),
        List.of(new ZoneDto(zoneId, "Scene", square(0, 0, 1))));

    Event event = EventMapper.toDomain(dto);

    assertEquals(eventId, event.getId());
    assertEquals("Roskilde", event.getName());
    assertEquals(START, event.getStartTime());
    assertEquals(END, event.getEndTime());
    assertEquals(Duration.ofSeconds(30), event.getSendInterval());
    assertTrue(event.getPerimeter().contains(new GeoPoint(2, 2)));
    assertFalse(event.getPerimeter().contains(new GeoPoint(5, 5)));

    Zone zone = event.getZones().getFirst();
    assertEquals(zoneId, zone.getId());
    assertEquals("Scene", zone.getName());
    assertTrue(zone.contains(new GeoPoint(0.5, 0.5)));
    assertFalse(zone.contains(new GeoPoint(2, 2)));
  }

  @Test
  void endBeforeStartIsRejected()
  {
    EventDto dto = new EventDto(UUID.randomUUID(), "Forkert", END, START, 30, square(0, 0, 4), List.of());

    assertThrows(IllegalArgumentException.class, () -> EventMapper.toDomain(dto));
  }

  /** Square with its south-west corner at (lat, lon) and sides of size degrees. */
  private static List<GeoPointDto> square(double lat, double lon, double size)
  {
    return List.of(
        new GeoPointDto(lat, lon),
        new GeoPointDto(lat, lon + size),
        new GeoPointDto(lat + size, lon + size),
        new GeoPointDto(lat + size, lon));
  }
}
