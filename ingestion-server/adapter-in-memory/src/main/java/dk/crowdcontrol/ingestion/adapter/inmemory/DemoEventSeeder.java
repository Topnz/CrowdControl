package dk.crowdcontrol.ingestion.adapter.inmemory;

import dk.crowdcontrol.ingestion.application.port.out.ActiveEventRepository;
import dk.crowdcontrol.ingestion.domain.Boundary;
import dk.crowdcontrol.ingestion.domain.Event;
import dk.crowdcontrol.ingestion.domain.GeoPoint;
import dk.crowdcontrol.ingestion.domain.Zone;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Component;

import java.time.Duration;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

/**
 * Puts a demo event in ActiveEventRepository at startup, so positions can be
 * accepted before processing-server sends real events.
 * Turned on and off with crowdcontrol.demo-event.enabled.
 */
@Component
@ConditionalOnProperty(name = "crowdcontrol.demo-event.enabled", havingValue = "true")
public class DemoEventSeeder implements ApplicationRunner
{
  private static final Logger log = LoggerFactory.getLogger(DemoEventSeeder.class);

  // Rectangle around Denmark, so positions from the whole country lie in the event
  private static final double DENMARK_SOUTH = 54.5;
  private static final double DENMARK_NORTH = 57.8;
  private static final double DENMARK_WEST = 8.0;
  private static final double DENMARK_EAST = 15.2;

  // Denmark split into two zones at the Great Belt, so zoneIds is not always empty
  private static final double ZONE_SPLIT_LONGITUDE = 11.0;

  // Fixed IDs, so C# can recognise the zones across restarts
  private static final UUID WEST_ZONE_ID = UUID.fromString("00000000-0000-0000-0000-00000000000a");
  private static final UUID EAST_ZONE_ID = UUID.fromString("00000000-0000-0000-0000-00000000000b");

  private final ActiveEventRepository repository;
  private final UUID eventId;
  private final Duration sendInterval;
  private final Duration duration;

  /**
   * @param repository   where the demo event is stored
   * @param eventId      the demo event's fixed ID (crowdcontrol.demo-event.id), which is in the mobile link
   * @param sendInterval the demo event's send interval (crowdcontrol.demo-event.send-interval)
   * @param duration     how long the demo event is active after startup (crowdcontrol.demo-event.duration)
   */
  public DemoEventSeeder(ActiveEventRepository repository,
      @Value("${crowdcontrol.demo-event.id}") UUID eventId,
      @Value("${crowdcontrol.demo-event.send-interval}") Duration sendInterval,
      @Value("${crowdcontrol.demo-event.duration}") Duration duration)
  {
    this.repository = repository;
    this.eventId = eventId;
    this.sendInterval = sendInterval;
    this.duration = duration;
  }

  /**
   * Creates the demo event covering all of Denmark with the zones Vestdanmark and Østdanmark,
   * and logs the link to the mobile app.
   *
   * @param args the program arguments (not used)
   */
  @Override public void run(ApplicationArguments args)
  {
    // Starts an hour before now, so phones with a slightly wrong clock are not rejected
    Instant start = Instant.now().minus(Duration.ofHours(1));
    Instant end = Instant.now().plus(duration);

    Zone west = new Zone(WEST_ZONE_ID, "Vestdanmark",
        rectangle(DENMARK_SOUTH, DENMARK_NORTH, DENMARK_WEST, ZONE_SPLIT_LONGITUDE));
    Zone east = new Zone(EAST_ZONE_ID, "Østdanmark",
        rectangle(DENMARK_SOUTH, DENMARK_NORTH, ZONE_SPLIT_LONGITUDE, DENMARK_EAST));

    Event event = new Event(eventId, "Demo-event", start, end, sendInterval,
        rectangle(DENMARK_SOUTH, DENMARK_NORTH, DENMARK_WEST, DENMARK_EAST),
        List.of(west, east));

    repository.createOrReplace(event);
    log.info("Demo-event oprettet: id={} aktivt til {}. Mobil-app: /?event={}", eventId, end, eventId);
  }

  /** Rectangle bounded by two latitudes (south/north) and two longitudes (west/east). */
  private static Boundary rectangle(double south, double north, double west, double east)
  {
    return new Boundary(List.of(
        new GeoPoint(south, west),
        new GeoPoint(south, east),
        new GeoPoint(north, east),
        new GeoPoint(north, west)));
  }
}
