package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.Event;

import java.util.List;

/**
 * Out-port: where ingestion-server gets the active events from processing-server.
 * Implemented by ProcessingActiveEventSource (REST, GET /events/active) and by
 * EmptyActiveEventSource with the "offline" profile.
 */
public interface ActiveEventSource
{
  /**
   * @return the upcoming and running events with their zones; empty if there are none
   */
  List<Event> fetchActiveEvents();
}
