package dk.crowdcontrol.ingestion.adapter.rest;

import dk.crowdcontrol.ingestion.application.port.in.OutsideActiveEventException;
import dk.crowdcontrol.ingestion.application.port.in.RateLimitExceededException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ProblemDetail;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

/**
 * Translates the use case's exceptions to the HTTP status codes promised by ingestion-api.yaml.
 * Invalid JSON and failed validation (@Valid) already give 400 from Spring itself.
 */
@RestControllerAdvice
public class RestExceptionHandler
{
  /**
   * @param e the device has sent too often
   * @return 429 Too Many Requests with the exception's message
   */
  @ExceptionHandler(RateLimitExceededException.class)
  public ProblemDetail handleRateLimitExceeded(RateLimitExceededException e)
  {
    return ProblemDetail.forStatusAndDetail(HttpStatus.TOO_MANY_REQUESTS, e.getMessage());
  }

  /**
   * @param e the event does not exist, or the position lies outside the area or time span
   * @return 422 Unprocessable Content with the exception's message
   */
  @ExceptionHandler(OutsideActiveEventException.class)
  public ProblemDetail handleOutsideActiveEvent(OutsideActiveEventException e)
  {
    return ProblemDetail.forStatusAndDetail(HttpStatus.UNPROCESSABLE_CONTENT, e.getMessage());
  }
}
