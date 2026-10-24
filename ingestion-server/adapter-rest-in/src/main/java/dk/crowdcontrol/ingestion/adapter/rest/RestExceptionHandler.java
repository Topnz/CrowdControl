package dk.crowdcontrol.ingestion.adapter.rest;

import dk.crowdcontrol.ingestion.application.port.in.OutsideActiveEventException;
import dk.crowdcontrol.ingestion.application.port.in.RateLimitExceededException;
import org.springframework.http.HttpStatus;
import org.springframework.http.ProblemDetail;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

/**
 * Oversætter use casens exceptions til de HTTP-koder, ingestion-api.yaml lover.
 * Ugyldig JSON og fejlet validering (@Valid) giver allerede 400 fra Spring selv.
 */
@RestControllerAdvice
public class RestExceptionHandler
{
  @ExceptionHandler(RateLimitExceededException.class)
  public ProblemDetail handleRateLimitExceeded(RateLimitExceededException e)
  {
    return ProblemDetail.forStatusAndDetail(HttpStatus.TOO_MANY_REQUESTS, e.getMessage());
  }

  @ExceptionHandler(OutsideActiveEventException.class)
  public ProblemDetail handleOutsideActiveEvent(OutsideActiveEventException e)
  {
    return ProblemDetail.forStatusAndDetail(HttpStatus.UNPROCESSABLE_CONTENT, e.getMessage());
  }
}
