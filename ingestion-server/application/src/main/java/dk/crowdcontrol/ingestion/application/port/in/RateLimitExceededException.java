package dk.crowdcontrol.ingestion.application.port.in;

/**
 * Thrown when a device (sessionId) sends positions more often than the rate limiter
 * allows. The position is not forwarded to processing-server.
 * The REST adapter responds with HTTP 429.
 */
public class RateLimitExceededException extends RuntimeException
{
  /** Creates the exception with a fixed message. */
  public RateLimitExceededException()
  {
    super("Rate Limit has been exceeded");
  }
}
