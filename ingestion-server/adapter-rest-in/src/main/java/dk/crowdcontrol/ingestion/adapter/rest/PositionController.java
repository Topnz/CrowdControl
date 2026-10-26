package dk.crowdcontrol.ingestion.adapter.rest;

import dk.crowdcontrol.ingestion.adapter.rest.dto.PositionRequest;
import dk.crowdcontrol.ingestion.application.port.in.ReceivePositionUseCase;
import jakarta.validation.Valid;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

/**
 * POST /positions fra mobil-appen. Fejl oversættes til HTTP-koder i RestExceptionHandler.
 */
@RestController
public class PositionController
{
  private final ReceivePositionUseCase receivePositionUseCase;

  public PositionController(ReceivePositionUseCase receivePositionUseCase)
  {
    this.receivePositionUseCase = receivePositionUseCase;
  }

  /** 202 Accepted: positionen er modtaget og sendt videre til processing-server. */
  @PostMapping("/positions")
  public ResponseEntity<Void> receivePosition(@Valid @RequestBody PositionRequest request)
  {
    receivePositionUseCase.receivePosition(request.toDomain(), request.eventId());
    return ResponseEntity.accepted().build();
  }
}
