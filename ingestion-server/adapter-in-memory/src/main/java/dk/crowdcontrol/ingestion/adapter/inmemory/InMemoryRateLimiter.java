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
 * Allows at most one position per sessionId within minInterval.
 * Remembers only in memory, so it works as long as a single ingestion-server is running.
 * Note: the map is never cleared, so it grows by one entry per device.
 */
@Component
public class InMemoryRateLimiter implements RateLimiter
{
  private final Map<SessionId, Instant> lastAllowed = new ConcurrentHashMap<>();
  private final Duration minInterval;

  /**
   * @param minInterval minimum time between two accepted positions from the same device
   *                    (crowdcontrol.rate-limit.min-interval)
   */
  public InMemoryRateLimiter(@Value("${crowdcontrol.rate-limit.min-interval}") Duration minInterval)
  {
    this.minInterval = minInterval;
  }

  /**
   * {@inheritDoc}
   * <p>
   * synchronized, so "read last time, compare, store new time" happens as one step.
   * Otherwise two simultaneous requests from the same device could both be allowed.
   */
  @Override public synchronized boolean allow(SessionId sessionId)
  {
    Instant now = Instant.now();
    Instant last = lastAllowed.get(sessionId);

    // First position from the device, or at least minInterval has passed since the last one
    if (last == null || !now.isBefore(last.plus(minInterval)))
    {
      lastAllowed.put(sessionId, now);
      return true;
    }
    return false;
  }
}
