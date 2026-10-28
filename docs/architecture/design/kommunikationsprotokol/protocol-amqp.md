# Protokol: AMQP via RabbitMQ (ingestion-server ↔ processing-server)

De to servere taler sammen gennem RabbitMQ (AMQP 0-9-1, `localhost:5672`, bruger `crowdcontrol`). Kontrakt: `contracts/asyncapi/messaging.yaml` og `contracts/schemas/rabbitMQ/`. PlantUML-versioner: `protocol-behandl-position.puml` og `protocol-event-changes.puml`. Overblik: `protocol-overview.md`.

- **Format:** `application/json` (Jackson i Java, `System.Text.Json` med web-defaults i C#). Tidspunkter er ISO 8601 i UTC.
- **Status:** Positions-køen (Java → C#) er implementeret. Exchangen `event-changes` (C# → Java) og `GET /events/active` ved opstart står i kontrakterne, men er ikke lavet endnu. Indtil da bruger ingestion-server et demo-event (`DemoEventSeeder`).

## Behandl position

```mermaid
sequenceDiagram
    participant Ing as Ingestion-server
    participant MQ as RabbitMQ
    participant Proc as Processing-server

    Proc-)MQ: 1: AMQP queue.declare("positions", durable)
    Proc-)MQ: 2: AMQP basic.qos(prefetch = 50)
    Proc-)MQ: 3: AMQP basic.consume("positions", autoAck = false)
    Note right of Proc: Prøver at forbinde hvert 5. sekund,<br/>indtil RabbitMQ svarer

    Ing-)MQ: 4: AMQP queue.declare("positions", durable)
    Note left of Ing: Ved første publish
    Ing-)MQ: 5: AMQP basic.publish("", "positions") JSON PositionReceived(sessionId,<br/>eventId, zoneIds, latitude, longitude, accuracy, timestamp)
    MQ-)Proc: 5.1: AMQP basic.deliver(deliveryTag) JSON PositionReceived(...)

    alt beskeden er gyldig og behandlet
        Proc-)MQ: 5.1.1: AMQP basic.ack(deliveryTag)
    else JSON følger ikke kontrakten, eller behandlingen fejler
        Proc-)MQ: 5.1.1: AMQP basic.nack(deliveryTag, requeue = false)
    end

    Note over MQ,Proc: Lukker serveren før ack, leverer RabbitMQ beskeden igen.<br/>Afviste beskeder kasseres.
```

## Event-ændringer (planlagt)

```mermaid
sequenceDiagram
    participant Ing as Ingestion-server
    participant MQ as RabbitMQ
    participant Proc as Processing-server
    participant Dash as Dashboard

    Ing->>Proc: 1: HTTP GET /events/active
    Proc-->>Ing: 2: HTTP 200 JSON List#lt;Event(eventId, name, startTime, endTime,<br/>sendIntervalSeconds, area, zones)#gt;
    Note left of Ing: Ved opstart

    Dash->>Proc: 3: Opret, ret eller slet event eller zone
    alt event oprettet
        Proc-)MQ: 3.1: AMQP basic.publish("event-changes", "event.created")<br/>JSON EventCreated(eventId, name, startTime, endTime, sendIntervalSeconds, area, zones)
        MQ-)Ing: 3.2: AMQP basic.deliver JSON EventCreated(...)
    else event eller zone ændret
        Proc-)MQ: 3.1: AMQP basic.publish("event-changes", "event.updated")<br/>JSON EventUpdated(eventId, name, startTime, endTime, sendIntervalSeconds, area, zones)
        MQ-)Ing: 3.2: AMQP basic.deliver JSON EventUpdated(...)
    else event slettet
        Proc-)MQ: 3.1: AMQP basic.publish("event-changes", "event.deleted")<br/>JSON EventDeleted(eventId)
        MQ-)Ing: 3.2: AMQP basic.deliver JSON EventDeleted(eventId)
    end

    Note over Ing,Dash: Planlagt – ikke implementeret endnu.<br/>Endpoints mellem dashboard og processing-server er ikke aftalt.<br/>En ændring skal nå ingestion-server inden for 30 sekunder (ID-35).
```

## Beskeder

**`PositionReceived`** på køen `positions` (alle felter påkrævet):

```json
{
  "sessionId": "3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b",
  "eventId": "00000000-0000-0000-0000-000000000001",
  "zoneIds": ["00000000-0000-0000-0000-00000000000a"],
  "latitude": 55.86,
  "longitude": 9.85,
  "accuracy": 8.5,
  "timestamp": "2026-10-24T18:22:18Z"
}
```

`zoneIds` er tom, hvis positionen ligger i eventet, men uden for alle zoner, og har flere elementer, hvis zonerne overlapper. Samme JSON bruges i `PosDataTest` (Java) og `PositionMessageTests` (C#), så begge sider testes mod samme besked.

**`EventCreated` / `EventUpdated`** (planlagt): hele eventet efter `Event` i `contracts/schemas/shared/common.v1.json`, dvs. `eventId`, `name`, `startTime`, `endTime`, `sendIntervalSeconds`, `area` (polygon) og `zones` (`zoneId`, `name`, `area`). Kapacitet sendes ikke, da den kun bruges i processing-server.

**`EventDeleted`** (planlagt): `{ "eventId": "…" }`.

| Kanal | Retning | AMQP | Besked | Status |
|---|---|---|---|---|
| `positions` | Java → C# | Default exchange, routing key = kønavn, durable kø | `PositionReceived` | Implementeret |
| `event-changes` | C# → Java | Exchange, routing key `event.created` / `event.updated` / `event.deleted` | `EventCreated` / `EventUpdated` / `EventDeleted` | Planlagt |
