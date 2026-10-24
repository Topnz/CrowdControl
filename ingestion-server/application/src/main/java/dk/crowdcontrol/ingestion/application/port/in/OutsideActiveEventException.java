package dk.crowdcontrol.ingestion.application.port.in;

/**
 * Kastes, når en position ikke kan knyttes til et aktivt event: eventet findes
 * ikke, eller positionen ligger uden for eventets område eller tidsrum.
 * Positionen sendes ikke videre til processing-server.
 * REST-adapteren svarer med HTTP 422.
 */
public class OutsideActiveEventException extends RuntimeException
{
  public OutsideActiveEventException(String message)
  {
    super(message);
  }
}
