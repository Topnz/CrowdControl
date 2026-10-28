package dk.crowdcontrol.ingestion.application.port.in;

/**
 * Thrown when a position cannot be linked to an active event: the event does not
 * exist, or the position lies outside the event's area or time span.
 * The position is not forwarded to processing-server.
 * The REST adapter responds with HTTP 422.
 */
public class OutsideActiveEventException extends RuntimeException
{
  /** @param message why the position was rejected; included in the response to the mobile app */
  public OutsideActiveEventException(String message)
  {
    super(message);
  }
}
