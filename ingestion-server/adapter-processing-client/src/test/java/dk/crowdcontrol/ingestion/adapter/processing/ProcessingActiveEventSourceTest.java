package dk.crowdcontrol.ingestion.adapter.processing;

import dk.crowdcontrol.ingestion.domain.Event;
import org.junit.jupiter.api.Test;
import org.springframework.http.HttpMethod;
import org.springframework.http.MediaType;
import org.springframework.test.web.client.MockRestServiceServer;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientException;

import java.time.Duration;
import java.util.List;
import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.method;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.requestTo;
import static org.springframework.test.web.client.response.MockRestResponseCreators.withServerError;
import static org.springframework.test.web.client.response.MockRestResponseCreators.withSuccess;

/**
 * Tests {@link ProcessingActiveEventSource} against a fake HTTP server (MockRestServiceServer):
 * that the JSON from GET /events/active (contracts/openapi/processing-api.yaml) is read and mapped,
 * that one invalid event is skipped without losing the others, and that HTTP errors are passed on.
 */
class ProcessingActiveEventSourceTest
{
  private static final String BASE_URL = "http://processing.test";
  private static final UUID VALID_ID = UUID.fromString("0190a1b2-0000-7000-8000-000000000001");
  private static final UUID INVALID_ID = UUID.fromString("0190a1b2-0000-7000-8000-000000000002");
  private static final UUID ZONE_ID = UUID.fromString("0190a1b2-0000-7000-8000-00000000000a");

  private final RestClient.Builder builder = RestClient.builder();
  private final MockRestServiceServer server = MockRestServiceServer.bindTo(builder).build();
  private final ProcessingActiveEventSource source = new ProcessingActiveEventSource(builder, BASE_URL);

  @Test
  void eventsAreReadFromJson()
  {
    expectActiveEvents("[" + eventJson(VALID_ID, "2026-07-01T10:00:00Z", "2026-07-01T22:00:00Z") + "]");

    List<Event> events = source.fetchActiveEvents();

    server.verify();
    assertEquals(1, events.size());
    Event event = events.getFirst();
    assertEquals(VALID_ID, event.getId());
    assertEquals("Roskilde", event.getName());
    assertEquals(Duration.ofSeconds(30), event.getSendInterval());
    assertEquals(ZONE_ID, event.getZones().getFirst().getId());
  }

  @Test
  void invalidEventIsSkippedAndOthersAreKept()
  {
    // INVALID_ID ends before it starts, which the domain rejects
    expectActiveEvents("["
        + eventJson(INVALID_ID, "2026-07-01T22:00:00Z", "2026-07-01T10:00:00Z") + ","
        + eventJson(VALID_ID, "2026-07-01T10:00:00Z", "2026-07-01T22:00:00Z") + "]");

    List<Event> events = source.fetchActiveEvents();

    assertEquals(List.of(VALID_ID), events.stream().map(Event::getId).toList());
  }

  @Test
  void emptyListGivesNoEvents()
  {
    expectActiveEvents("[]");

    assertTrue(source.fetchActiveEvents().isEmpty());
  }

  @Test
  void serverErrorIsPassedOn()
  {
    server.expect(requestTo(BASE_URL + "/events/active")).andRespond(withServerError());

    assertThrows(RestClientException.class, source::fetchActiveEvents);
  }

  /** Makes the fake server answer GET /events/active with the given JSON. */
  private void expectActiveEvents(String json)
  {
    server.expect(requestTo(BASE_URL + "/events/active"))
        .andExpect(method(HttpMethod.GET))
        .andRespond(withSuccess(json, MediaType.APPLICATION_JSON));
  }

  /** Event as JSON per Event in contracts/schemas/shared/common.v1.json, with one zone. */
  private static String eventJson(UUID id, String start, String end)
  {
    String square = """
        [{"latitude":0,"longitude":0},{"latitude":0,"longitude":1},
         {"latitude":1,"longitude":1},{"latitude":1,"longitude":0}]""";
    return """
        {"eventId":"%s","name":"Roskilde","startTime":"%s","endTime":"%s",
         "sendIntervalSeconds":30,"area":%s,
         "zones":[{"zoneId":"%s","name":"Scene","area":%s}]}"""
        .formatted(id, start, end, square, ZONE_ID, square);
  }
}
