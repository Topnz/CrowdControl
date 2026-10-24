package dk.crowdcontrol.ingestion.application.port.in;

import dk.crowdcontrol.ingestion.domain.Position;

import java.util.UUID;

public interface ReceivePositionUseCase
{
  /**
   * Modtager en position fra mobil-appen og sender den videre til processing-server,
   * hvis den ligger inden for eventets område og tidsrum.
   *
   * @param position positionen med det anonyme sessionId
   * @param eventId  det event, mobil-appen sender til
   * @throws RateLimitExceededException hvis enheden sender oftere, end rate limiteren tillader
   * @throws OutsideActiveEventException hvis eventet ikke findes, eller positionen ligger
   *                                     uden for eventets område eller tidsrum
   */
  void receivePosition(Position position, UUID eventId);
}
