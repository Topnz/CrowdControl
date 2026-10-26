package dk.crowdcontrol.ingestion.adapter.inmemory;

import dk.crowdcontrol.ingestion.application.port.out.RateLimiter;
import dk.crowdcontrol.ingestion.domain.SessionId;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import java.time.Duration;
import java.time.Instant;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Tillader højst én position pr. sessionId inden for minInterval.
 * Husker kun i hukommelsen, så det virker, så længe der kører én ingestion-server.
 * Bemærk: mappet ryddes ikke, så det vokser med én linje pr. enhed.
 */
@Component
public class InMemoryRateLimiter implements RateLimiter
{
  private final Map<SessionId, Instant> lastAllowed = new ConcurrentHashMap<>();
  private final Duration minInterval;

  public InMemoryRateLimiter(@Value("${crowdcontrol.rate-limit.min-interval}") Duration minInterval)
  {
    this.minInterval = minInterval;
  }

  /**
   * synchronized, så "læs sidste tid, sammenlign, gem ny tid" sker samlet.
   * Ellers kunne to samtidige requests fra samme enhed begge få lov.
   */
  @Override public synchronized boolean allow(SessionId sessionId)
  {
    Instant now = Instant.now();
    Instant last = lastAllowed.get(sessionId);

    // Første position fra enheden, eller der er gået mindst minInterval siden sidst
    if (last == null || !now.isBefore(last.plus(minInterval)))
    {
      lastAllowed.put(sessionId, now);
      return true;
    }
    return false;
  }
}
