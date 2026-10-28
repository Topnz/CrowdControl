package dk.crowdcontrol.ingestion.adapter.rest;

import dk.crowdcontrol.ingestion.adapter.rest.dto.PositionRequest;
import dk.crowdcontrol.ingestion.application.port.in.ReceivePositionUseCase;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

/**
 * POST /positions from the mobile app. Errors are translated to HTTP status codes in RestExceptionHandler.
 */
@RestController
public class PositionController
{
  private final ReceivePositionUseCase receivePositionUseCase;

  /** @param receivePositionUseCase the use case that accepts the position */
  public PositionController(ReceivePositionUseCase receivePositionUseCase)
  {
    this.receivePositionUseCase = receivePositionUseCase;
  }

  /**
   * Accepts a position from the mobile app.
   * An invalid body gives 400, too many positions 429, and outside the event 422.
   *
   * @param request the position and the event's ID, validated with @Valid
   * @return 202 Accepted: the position has been received and forwarded to processing-server
   */
  @PostMapping("/positions")
  public ResponseEntity<Void> receivePosition(@Valid @RequestBody PositionRequest request)
  {
    receivePositionUseCase.receivePosition(request.toDomain(), request.eventId());
    return ResponseEntity.accepted().build();
  }
}
