package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.FilteredPosition;

/**
 * Out-port: forwards accepted positions to processing-server.
 * RabbitPositionPublisher sends to the positions queue; LoggingPositionPublisher
 * writes them to the log instead with the "offline" profile.
 */
public interface PositionPublisher
{
  /** @param filteredPosition the position with eventId and the zones it lies in */
  void publish(FilteredPosition filteredPosition);
}
