package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.SessionId;

/**
 * Out-port: limits how often one device may send positions (ID-28), so
 * the overview cannot be manipulated.
 */
public interface RateLimiter
{
  /**
   * Registers a position from the device and decides whether it may be accepted.
   *
   * @param sessionId the device's session ID
   * @return true if the position may be accepted; false if the device has sent too often
   */
  boolean allow(SessionId sessionId);
}
