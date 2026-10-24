package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.SessionId;

public interface RateLimiter
{
  boolean allow(SessionId sessionId);
}
