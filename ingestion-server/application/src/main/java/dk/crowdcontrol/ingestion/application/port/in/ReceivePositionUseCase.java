package dk.crowdcontrol.ingestion.application.port.in;

import dk.crowdcontrol.ingestion.domain.Position;

import java.util.UUID;

/**
 * In-port: accept a position from the mobile app (UC2 Share position).
 * Called by the REST adapter and implemented by ReceivePositionService.
 */
public interface ReceivePositionUseCase
{
  /**
   * Receives a position from the mobile app and forwards it to processing-server
   * if it lies within the event's area and time span.
   *
   * @param position the position with the pseudonymous sessionId
   * @param eventId  the event the mobile app is sending to
   * @throws RateLimitExceededException if the device sends more often than the rate limiter allows
   * @throws OutsideActiveEventException if the event does not exist, or the position lies
   *                                     outside the event's area or time span
   */
  void receivePosition(Position position, UUID eventId);
}
