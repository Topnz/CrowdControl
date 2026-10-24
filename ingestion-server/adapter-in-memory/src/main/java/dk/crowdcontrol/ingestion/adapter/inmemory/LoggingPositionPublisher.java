package dk.crowdcontrol.ingestion.adapter.inmemory;

import dk.crowdcontrol.ingestion.application.port.out.PositionPublisher;
import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import dk.crowdcontrol.ingestion.domain.Position;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;

/**
 * Skriver godkendte positioner til loggen i stedet for at sende dem til RabbitMQ.
 * Kun med profilen "local", så Java kan køre uden RabbitMQ og processing-server.
 */
@Component
@Profile("local")
public class LoggingPositionPublisher implements PositionPublisher
{
  private static final Logger log = LoggerFactory.getLogger(LoggingPositionPublisher.class);

  @Override public void publish(FilteredPosition filteredPosition)
  {
    Position position = filteredPosition.getPosition();
    log.info("Position: sessionId={} eventId={} zoneIds={} lat={} lon={} accuracy={}m timestamp={}",
        position.getSessionId(),
        filteredPosition.getEventId(),
        filteredPosition.getZoneIds(),
        position.getGeoPoint().getLatitude(),
        position.getGeoPoint().getLongitude(),
        position.getAccuracy(),
        position.getTimestamp());
  }
}
