# Kommunikationsprotokol

Overblik over al kommunikation mellem mobil-appen, ingestion-server, RabbitMQ og processing-server. Tegnet i samme stil som `Kommunikationsprotokol.asta`: nummererede beskeder, hvor hver besked viser protokol, format og indhold. PlantUML-version: `protocol-overview.puml` (stil i `astah-style.iuml`).

Detaljer pr. protokol:

- `protocol-ingestion-api.md` – HTTP mellem mobil-app og ingestion-server (*Send position*, *Send position_Fejl*)
- `protocol-amqp.md` – AMQP mellem serverne (*Behandl position*, *Event-ændringer*)

```mermaid
sequenceDiagram
    participant App as Mobil-app
    participant Ing as Ingestion-server
    participant MQ as RabbitMQ
    participant Proc as Processing-server

    App->>Ing: 1: HTTP GET /?event={eventId}
    Ing-->>App: 2: HTTP 200 index.html, app.js, styles.css

    App->>Ing: 3: HTTP GET /events/{eventId}
    Ing-->>App: 4: HTTP 200 JSON EventInfo(eventId, name, startTime, endTime, sendIntervalSeconds)
    Note right of Ing: 3–4 er planlagt (ID-21, ID-29)

    App->>Ing: 5: HTTP POST /positions JSON PositionRequest(sessionId, eventId,<br/>latitude, longitude, accuracy, timestamp)
    Ing-)MQ: 5.1: AMQP basic.publish("", "positions") JSON PositionReceived(sessionId,<br/>eventId, zoneIds, latitude, longitude, accuracy, timestamp)
    Ing-->>App: 6: HTTP 202 Accepted
    MQ-)Proc: 5.2: AMQP basic.deliver(deliveryTag) JSON PositionReceived(...)
    Proc-)MQ: 5.3: AMQP basic.ack(deliveryTag)

    App->>Ing: 7: HTTP #lt;metode#gt; #lt;sti#gt; JSON #lt;request#gt;
    Ing-->>App: 8: HTTP 4xx JSON ProblemDetail(status, detail)
    Note right of Ing: Gælder alle HTTP-kald

    Ing->>Proc: 9: HTTP GET /events/active
    Proc-->>Ing: 10: HTTP 200 JSON List#lt;Event(eventId, name, startTime, endTime,<br/>sendIntervalSeconds, area, zones)#gt;
    Proc-)MQ: 11: AMQP basic.publish("event-changes", "event.#lt;type#gt;") JSON Event / EventDeleted(eventId)
    MQ-)Ing: 11.1: AMQP basic.deliver(deliveryTag) JSON Event / EventDeleted(eventId)
    Note over Ing,Proc: 9–11.1 er planlagt (ID-35)
```

**Notation** (som i Astah):

- Fuld pil med lukket spids: synkront kald (HTTP-request). Stiplet pil: svar.
- Pil med åben spids: asynkron besked (AMQP).
- `1`, `2` … er rækkefølgen; `5.1` er en besked, der sendes som følge af `5`.
- Lyserøde noter forklarer, hvad der gælder flere beskeder, eller hvad der er planlagt.
