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
 * Lægger et demo-event i ActiveEventRepository ved opstart, så positioner kan
 * godkendes, før processing-server sender rigtige events.
 * Slås til og fra med crowdcontrol.demo-event.enabled.
 */
@Component
@ConditionalOnProperty(name = "crowdcontrol.demo-event.enabled", havingValue = "true")
public class DemoEventSeeder implements ApplicationRunner
{
  private static final Logger log = LoggerFactory.getLogger(DemoEventSeeder.class);

  // Firkant omkring Danmark, så positioner fra hele landet ligger i eventet
  private static final double DENMARK_SOUTH = 54.5;
  private static final double DENMARK_NORTH = 57.8;
  private static final double DENMARK_WEST = 8.0;
  private static final double DENMARK_EAST = 15.2;

  // Danmark delt i to zoner ved Storebælt, så zoneIds ikke altid er tom
  private static final double ZONE_SPLIT_LONGITUDE = 11.0;

  // Faste ID'er, så C# kan genkende zonerne på tværs af genstarter
  private static final UUID WEST_ZONE_ID = UUID.fromString("00000000-0000-0000-0000-00000000000a");
  private static final UUID EAST_ZONE_ID = UUID.fromString("00000000-0000-0000-0000-00000000000b");

  private final ActiveEventRepository repository;
  private final UUID eventId;
  private final Duration sendInterval;
  private final Duration duration;

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

  @Override public void run(ApplicationArguments args)
  {
    // Starter en time før nu, så telefoner med lidt forkert ur ikke afvises
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

  private static Boundary rectangle(double south, double north, double west, double east)
  {
    return new Boundary(List.of(
        new GeoPoint(south, west),
        new GeoPoint(south, east),
        new GeoPoint(north, east),
        new GeoPoint(north, west)));
  }
}
