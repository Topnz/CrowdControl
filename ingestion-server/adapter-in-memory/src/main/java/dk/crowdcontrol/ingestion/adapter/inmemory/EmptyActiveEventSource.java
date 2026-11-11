package dk.crowdcontrol.ingestion.adapter.inmemory;

import dk.crowdcontrol.ingestion.application.port.out.ActiveEventSource;
import dk.crowdcontrol.ingestion.domain.Event;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;

import java.util.List;

/**
 * Returns no events, since processing-server is not running in the "offline" profile.
 * The demo event comes from DemoEventSeeder instead.
 * Only with the "offline" profile; otherwise ProcessingActiveEventSource is used.
 */
@Component
@Profile("offline")
public class EmptyActiveEventSource implements ActiveEventSource
{
  /** @return an empty list */
  @Override public List<Event> fetchActiveEvents()
  {
    return List.of();
  }
}
