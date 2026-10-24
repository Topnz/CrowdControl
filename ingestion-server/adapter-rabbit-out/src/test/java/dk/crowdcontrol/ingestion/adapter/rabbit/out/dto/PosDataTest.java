package dk.crowdcontrol.ingestion.adapter.rabbit.out.dto;

import dk.crowdcontrol.ingestion.domain.FilteredPosition;
import dk.crowdcontrol.ingestion.domain.GeoPoint;
import dk.crowdcontrol.ingestion.domain.Position;
import dk.crowdcontrol.ingestion.domain.SessionId;
import org.junit.jupiter.api.Test;
import org.springframework.amqp.core.Message;
import org.springframework.amqp.core.MessageProperties;
import org.springframework.amqp.support.converter.JacksonJsonMessageConverter;

import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.List;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * Tjekker, at beskeden bliver til den JSON, C# forventer (PositionReceived.v1.json):
 * rigtige feltnavne og tid som ISO 8601-tekst, ikke som et tal.
 */
class PosDataTest
{
  @Test
  void jsonMatchesContract()
  {
    UUID sessionId = UUID.fromString("3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b");
    UUID eventId = UUID.fromString("00000000-0000-0000-0000-000000000001");
    UUID zoneId = UUID.fromString("00000000-0000-0000-0000-00000000000a");
    Position position = new Position(new SessionId(sessionId), new GeoPoint(55.86, 9.85), 8.5,
        Instant.parse("2026-10-24T18:22:18Z"));
    PosData posData = PosData.from(new FilteredPosition(position, eventId, List.of(zoneId)));

    Message message = new JacksonJsonMessageConverter().toMessage(posData, new MessageProperties());
    String json = new String(message.getBody(), StandardCharsets.UTF_8);

    assertEquals("{\"sessionId\":\"3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b\","
        + "\"eventId\":\"00000000-0000-0000-0000-000000000001\","
        + "\"zoneIds\":[\"00000000-0000-0000-0000-00000000000a\"],"
        + "\"latitude\":55.86,\"longitude\":9.85,\"accuracy\":8.5,"
        + "\"timestamp\":\"2026-10-24T18:22:18Z\"}", json);
    assertEquals("application/json", message.getMessageProperties().getContentType());
  }
}
