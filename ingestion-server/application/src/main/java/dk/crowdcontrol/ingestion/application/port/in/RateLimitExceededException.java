package dk.crowdcontrol.ingestion.application.port.in;

/**
 * Kastes, når en enhed (sessionId) sender positioner oftere, end rate limiteren
 * tillader. Positionen sendes ikke videre til processing-server.
 * REST-adapteren svarer med HTTP 429.
 */
public class RateLimitExceededException extends RuntimeException
{
  public RateLimitExceededException()
  {
    super("Rate Limit has been exceeded");
  }
}
