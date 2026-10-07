# CrowdControl – projektkontekst

Denne fil samler arkitektur, beslutninger og opsætning for CrowdControl-projektet (SEP3). Den er skrevet til at ligge i repo-roden som `CLAUDE.md`, så Claude Code automatisk læser den.

---

## 1. Systemet (fra C2-diagrammet)

CrowdControl viser et live heatmap over, hvor deltagere befinder sig til et event (fx en festival).

**Personer**

- **Event-Participant** – deltager i eventet, giver samtykke og deler sin position via mobil-appen.
- **Event Holder** – medarbejder hos arrangøren, der overvåger eventet og opretter events/zoner i dashboardet.

**Containere**

| Container | Teknologi | Ansvar |
|---|---|---|
| Mobile app | JavaScript, HTML (mobilbrowser) | Læser position via Geolocation API i et givent interval og sender den anonymt. Serveres som statisk indhold af ingestion-server. |
| Ingestion-server | Java, Spring Boot | Modtager positioner, validerer dem mod aktive events og eventområder, rate-limiter og publicerer til køen. Serverer mobil-appen. |
| Event-database | PostgreSQL + PostGIS | Kopi af aktive events: ID, tidsrum og område. Ejes af ingestion-server. |
| Message broker | RabbitMQ | Kø med positioner (Java → C#) og exchange med event-ændringer (C# → Java). |
| Processing-server | C#, ASP.NET Core | Kunder, login, events, aggregering til grid-celler, historik, zoner. Serverer dashboardet. |
| Main Database | PostgreSQL | Tabeller: customers, event hosts, users, events, heatmap, zones. Ejes af processing-server. |
| Dashboard | C# Blazor WebAssembly | Live heatmap og admin-panel til oprettelse af zoner m.m. |

**Kommunikation**

| Fra → Til | Hvad | Protokol |
|---|---|---|
| Mobile app → Ingestion-server | Sender positioner | REST, JSON/HTTPS |
| Mobile app → Ingestion-server | Henter statisk indhold | HTTPS |
| Ingestion-server → Event-database | Læser/skriver aktive events og eventområder | JDBC, SQL |
| Ingestion-server → RabbitMQ | Publicerer positioner | AMQP |
| RabbitMQ → Processing-server | Leverer positioner | AMQP |
| Processing-server → RabbitMQ | Publicerer EventCreated / EventUpdated / EventDeleted | AMQP |
| RabbitMQ → Ingestion-server | Leverer event-ændringer | AMQP |
| Ingestion-server → Processing-server | Henter aktive events ved opstart | REST, JSON/HTTPS |
| Processing-server → Main Database | Læser/skriver kunder, events, heatmap | EF Core, SQL |
| Processing-server → Dashboard | Pusher live-opdateringer | SignalR, WebSocket |
| Dashboard → Processing-server | Henter historik | REST, JSON/HTTPS |
| Dashboard → Processing-server | Henter statisk indhold | HTTPS |

---

## 2. Grundprincip: uafhængigt arbejde via Dependency Inversion

D'et i SOLID er *Dependency Inversion*: kode afhænger af abstraktioner (interfaces), ikke af konkrete implementeringer. *Dependency Injection* er værktøjet, der leverer implementeringen udefra. Målet er, at alle kan arbejde uafhængigt af hinanden på tre niveauer:

1. **Mellem containerne** – Java og C# må ikke vente på hinanden. Løses med fælles kontrakter i `contracts/` (contract-first). Hver side bygger mod en fake af den anden.
2. **Inde i hver container** – Ports & Adapters (hexagonal arkitektur). Hver adapter er sit eget modul, så den, der laver RabbitMQ, ikke træder på den, der laver databasen.
3. **Frontends mod backends** – mobil-appen og dashboardet udvikles mod fakes/mocks.

**Afhængighedsreglen:** alle pile peger indad.

```
domain  ←  application  ←  adapters  ←  bootstrap (composition root)
```

- `domain` har ingen afhængigheder (ingen Spring, ingen JDBC, ingen RabbitMQ).
- `application` afhænger kun af `domain` og definerer porte (interfaces).
- Hver adapter afhænger kun af `application` plus sin egen teknologi.
- Kun `bootstrap` (Java) / `Api` (C#) kender alle moduler og laver DI-wiringen.

Reglen håndhæves af byggesystemet (Maven-moduler / .NET project references), ikke kun af aftale. Hvis nogen importerer en Spring-klasse i `domain` eller `application`, skal compileren fejle.

**Hver service ejer sin egen database og sine egne migrationer.** Java: Flyway til Event-database. C#: EF Core migrations til Main Database. Ingen delt database.

---

## 3. Repo-struktur (monorepo)

```
CrowdControl/
├── CLAUDE.md
├── README.md
├── docker-compose.yml               # RabbitMQ + main-db + event-db (PostGIS)
├── .gitignore  .editorconfig
├── docs/architecture/
│   └── C2-centrale-teknologier.drawio
│
├── contracts/                       # Det ENESTE der deles på tværs af containere
│   ├── openapi/
│   │   ├── ingestion-api.yaml       # POST /positions            (mobil → Java)
│   │   └── processing-api.yaml      # GET /events/active, /history (→ C#)
│   ├── asyncapi/
│   │   └── messaging.yaml           # positions-kø + event-exchange
│   └── schemas/
│       ├── PositionReceived.v1.json
│       ├── EventCreated.v1.json
│       ├── EventUpdated.v1.json
│       └── EventDeleted.v1.json
│
├── mobile-app/                      # JS/HTML, Vite – egen dev-server mod mock-API
│   ├── src/
│   │   ├── api/positionClient.js    # interface + fetch-impl + fake-impl
│   │   ├── geo/locationProvider.js  # wrapper om Geolocation API (kan fakes)
│   │   └── app.js
│   └── package.json                 # build → kopieres ind i bootstrap/static
│
├── ingestion-server/                # Java 21, Spring Boot 4.1.1, Maven multi-module
│   ├── pom.xml                      # parent, packaging=pom
│   ├── mvnw  mvnw.cmd  .mvn/
│   ├── domain/
│   ├── application/
│   ├── adapter-rest-in/             # @RestController POST /positions
│   ├── adapter-rabbit-in/           # lytter på EventCreated/Updated/Deleted
│   ├── adapter-rabbit-out/          # RabbitPositionPublisher
│   ├── adapter-persistence-postgis/ # JDBC + Flyway-migrationer til event-db
│   ├── adapter-processing-client/   # REST: hent aktive events ved opstart
│   ├── adapter-in-memory/           # fakes af alle out-porte til lokal kørsel
│   └── bootstrap/                   # @SpringBootApplication + @Configuration
│       └── src/main/resources/static/   # ← mobile-app build lander her
│
└── processing-server/               # C#, .NET solution (åbnes i Rider)
    ├── CrowdControl.Processing.sln
    ├── src/
    │   ├── CrowdControl.Domain/                     # grid-aggregering, entiteter
    │   ├── CrowdControl.Application/                # use cases + interfaces
    │   ├── CrowdControl.Infrastructure.Persistence/ # EF Core + migrationer (main-db)
    │   ├── CrowdControl.Infrastructure.Messaging/   # RabbitMQ consumer/publisher
    │   ├── CrowdControl.Shared/                     # DTO'er delt af Api og Dashboard
    │   ├── CrowdControl.Dashboard/                  # Blazor WASM
    │   └── CrowdControl.Api/                        # host: REST, SignalR-hub,
    │                                                # Program.cs, serverer Dashboard
    └── tests/
        ├── CrowdControl.Domain.Tests/
        ├── CrowdControl.Application.Tests/
        └── CrowdControl.Api.IntegrationTests/
```

Java-adapterne ligger fladt med `adapter-`-præfiks (ikke i en `adapters/`-undermappe). Det er nemmere i IntelliJ, og de sorteres sammen alfabetisk.

---

## 4. IDE-opsætning

- Roden `CrowdControl/` er oprettet i IntelliJ som **Empty Project** med git-repository.
- `ingestion-server/` er tilføjet via **File → New → Module → Spring Boot** (Maven, Java 21, JDK liberica-21, groupId `dk.crowdcontrol`).
- IntelliJ kan ikke arbejde ordentligt med C#. `processing-server/` åbnes i **Rider** (`CrowdControl.Processing.sln`). Begge IDE'er peger på samme repo.
- `mobile-app/` oprettes med Vite (`npm create vite@latest mobile-app -- --template vanilla`) eller via IntelliJ's Vite-generator.
- Udviklingsmiljøet er Windows. Maven køres med `.\mvnw.cmd` fra `ingestion-server/`.

---

## 5. Ingestion-server (Java) – Maven multi-module

### 5.1 Parent-pom (`ingestion-server/pom.xml`)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 https://maven.apache.org/xsd/maven-4.0.0.xsd">
    <modelVersion>4.0.0</modelVersion>

    <parent>
        <groupId>org.springframework.boot</groupId>
        <artifactId>spring-boot-starter-parent</artifactId>
        <version>4.1.1</version>
        <relativePath/>
    </parent>

    <groupId>dk.crowdcontrol</groupId>
    <artifactId>ingestion-server</artifactId>
    <version>0.0.1-SNAPSHOT</version>
    <packaging>pom</packaging>
    <name>ingestion-server</name>

    <properties>
        <java.version>21</java.version>
    </properties>

    <modules>
        <module>domain</module>
        <module>application</module>
        <module>adapter-rest-in</module>
        <module>adapter-rabbit-in</module>
        <module>adapter-rabbit-out</module>
        <module>adapter-persistence-postgis</module>
        <module>adapter-processing-client</module>
        <module>adapter-in-memory</module>
        <module>bootstrap</module>
    </modules>

    <dependencyManagement>
        <dependencies>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>domain</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>application</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>adapter-rest-in</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>adapter-rabbit-in</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>adapter-rabbit-out</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>adapter-persistence-postgis</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>adapter-processing-client</artifactId><version>${project.version}</version></dependency>
            <dependency><groupId>dk.crowdcontrol</groupId><artifactId>adapter-in-memory</artifactId><version>${project.version}</version></dependency>
        </dependencies>
    </dependencyManagement>
</project>
```

Parent-pom'en har **ingen** `<dependencies>` og **ingen** `spring-boot-maven-plugin`. Versioner på Spring-artefakter arves fra `spring-boot-starter-parent`; interne moduler får version fra `<dependencyManagement>`.

### 5.2 Hvert moduls dependencies

Hvert modul har `<parent>` = `dk.crowdcontrol:ingestion-server:0.0.1-SNAPSHOT`. Spring Boot 4 har opdelte starters (fx `spring-boot-starter-webmvc`, ikke `-web`) med hver sin `-test`-starter.

| Modul | Main-dependencies | Test-dependencies |
|---|---|---|
| `domain` | *(ingen)* | `org.junit.jupiter:junit-jupiter` |
| `application` | `domain` | `org.junit.jupiter:junit-jupiter` |
| `adapter-rest-in` | `application`, `spring-boot-starter-webmvc`, `spring-boot-starter-validation` | `spring-boot-starter-webmvc-test`, `spring-boot-starter-validation-test` |
| `adapter-rabbit-in` | `application`, `spring-boot-starter-amqp` | `spring-boot-starter-amqp-test` |
| `adapter-rabbit-out` | `application`, `spring-boot-starter-amqp` | `spring-boot-starter-amqp-test` |
| `adapter-persistence-postgis` | `application`, `spring-boot-starter-jdbc`, `spring-boot-starter-flyway`, `org.flywaydb:flyway-database-postgresql`, `org.postgresql:postgresql` (scope runtime) | `spring-boot-starter-jdbc-test`, `spring-boot-starter-flyway-test` |
| `adapter-processing-client` | `application`, `spring-boot-starter-restclient` | – |
| `adapter-in-memory` | `application`, `spring-boot-starter` | – |
| `bootstrap` | `application`, alle `adapter-*`, `spring-boot-starter` | `spring-boot-starter-test` |

Kun `bootstrap` har dette i sin pom, så kun det modul bygges til en kørbar jar:

```xml
<build>
    <plugins>
        <plugin>
            <groupId>org.springframework.boot</groupId>
            <artifactId>spring-boot-maven-plugin</artifactId>
        </plugin>
    </plugins>
</build>
```

Flyway-migrationer ligger i `adapter-persistence-postgis/src/main/resources/db/migration/`. De kommer med på classpath, og Spring Boot finder dem automatisk.

### 5.3 Pakkestruktur

`@SpringBootApplication` scanner kun sin egen pakke og underpakker. Application-klassen skal derfor ligge øverst, og hvert modul har sin egen underpakke (ingen delte pakker mellem moduler):

| Pakke | Modul |
|---|---|
| `dk.crowdcontrol.ingestion` | Application-klassen i `bootstrap` |
| `dk.crowdcontrol.ingestion.domain` | `domain` |
| `dk.crowdcontrol.ingestion.application` | `application` (fx `.port.in`, `.port.out`, `.service`) |
| `dk.crowdcontrol.ingestion.adapter.rest` | `adapter-rest-in` |
| `dk.crowdcontrol.ingestion.adapter.rabbit` | `adapter-rabbit-in` / `adapter-rabbit-out` (brug evt. `.in` / `.out`) |
| `dk.crowdcontrol.ingestion.adapter.persistence` | `adapter-persistence-postgis` |
| `dk.crowdcontrol.ingestion.adapter.processing` | `adapter-processing-client` |
| `dk.crowdcontrol.ingestion.adapter.inmemory` | `adapter-in-memory` |

### 5.4 Domæne og porte

- **Domain:** `Position`, `ActiveEvent`, `EventArea`, `GeoPoint` …
- **In-porte:** `ReceivePositionUseCase`, `ApplyEventChangeUseCase`
- **Out-porte:** `PositionPublisher`, `ActiveEventRepository`, `ActiveEventSource`, `RateLimiter`
- **Services:** `ReceivePositionService`, …

`ActiveEventSource` dækker både "hent aktive events fra C# ved opstart" (REST-adapter) og "lyt på ændringer" (Rabbit-adapter), med en in-memory-fake til lokal kørsel.

### 5.5 Eksempel: port, service og wiring

Porten ligger i `application` og ejes af den, der bruger den:

```java
public interface PositionPublisher {
    void publish(Position position);
}

public class ReceivePositionService implements ReceivePositionUseCase {
    private final ActiveEventRepository events;
    private final PositionPublisher publisher;
    private final RateLimiter rateLimiter;

    public ReceivePositionService(ActiveEventRepository events,
                                  PositionPublisher publisher,
                                  RateLimiter rateLimiter) {
        this.events = events;
        this.publisher = publisher;
        this.rateLimiter = rateLimiter;
    }

    @Override
    public void receive(Position p) {
        if (!rateLimiter.allow(p.sessionId())) throw new RateLimitExceeded();
        if (!events.isInsideActiveEvent(p))   throw new OutsideActiveEvent();
        publisher.publish(p);
    }
}
```

Servicen har **ingen** Spring-annotationer. Wiringen sker i `bootstrap`:

```java
@Configuration
class UseCaseConfig {
    @Bean
    ReceivePositionUseCase receivePosition(ActiveEventRepository e,
                                           PositionPublisher p,
                                           RateLimiter r) {
        return new ReceivePositionService(e, p, r);
    }
}
```

Profiler afgør, hvilken implementering der bruges:

```java
// adapter-rabbit-out
@Component @Profile("!local")
class RabbitPositionPublisher implements PositionPublisher { ... }

// adapter-in-memory
@Component @Profile("local")
class LoggingPositionPublisher implements PositionPublisher { ... }
```

Med `--spring.profiles.active=local` skal Java-serveren kunne køre helt uden RabbitMQ, PostGIS og C#-serveren. Bemærk: `local`-profilen skal også slå DataSource- og Flyway-autokonfiguration fra (fx via `spring.autoconfigure.exclude` i `application-local.properties`), ellers forsøger Boot at forbinde til databasen ved opstart. Tjek de præcise klassenavne for Spring Boot 4, da autokonfigurationerne er flyttet til nye pakker.

---

## 6. Processing-server (C#) – .NET solution

### 6.1 Oprettelse med `dotnet` CLI

Kør fra `CrowdControl/processing-server`:

```bash
dotnet new sln -n CrowdControl.Processing

dotnet new classlib    -n CrowdControl.Domain                     -o src/CrowdControl.Domain
dotnet new classlib    -n CrowdControl.Application                -o src/CrowdControl.Application
dotnet new classlib    -n CrowdControl.Infrastructure.Persistence -o src/CrowdControl.Infrastructure.Persistence
dotnet new classlib    -n CrowdControl.Infrastructure.Messaging   -o src/CrowdControl.Infrastructure.Messaging
dotnet new classlib    -n CrowdControl.Shared                     -o src/CrowdControl.Shared
dotnet new blazorwasm  -n CrowdControl.Dashboard                  -o src/CrowdControl.Dashboard
dotnet new web         -n CrowdControl.Api                        -o src/CrowdControl.Api

dotnet new xunit -n CrowdControl.Domain.Tests          -o tests/CrowdControl.Domain.Tests
dotnet new xunit -n CrowdControl.Application.Tests     -o tests/CrowdControl.Application.Tests
dotnet new xunit -n CrowdControl.Api.IntegrationTests  -o tests/CrowdControl.Api.IntegrationTests

dotnet sln add src/*/*.csproj tests/*/*.csproj
```

### 6.2 Project references (følger afhængighedsreglen)

```bash
dotnet add src/CrowdControl.Application reference src/CrowdControl.Domain
dotnet add src/CrowdControl.Infrastructure.Persistence reference src/CrowdControl.Application
dotnet add src/CrowdControl.Infrastructure.Messaging   reference src/CrowdControl.Application
dotnet add src/CrowdControl.Dashboard reference src/CrowdControl.Shared
dotnet add src/CrowdControl.Api reference \
  src/CrowdControl.Application \
  src/CrowdControl.Infrastructure.Persistence \
  src/CrowdControl.Infrastructure.Messaging \
  src/CrowdControl.Shared \
  src/CrowdControl.Dashboard
```

For at `Api` kan servere dashboardet:

1. Tilføj pakken `Microsoft.AspNetCore.Components.WebAssembly.Server` til `CrowdControl.Api`.
2. Kald `app.UseBlazorFrameworkFiles()` og `app.MapFallbackToFile("index.html")` i `Program.cs`.

### 6.3 Porte og composition root

Hvert Infrastructure-projekt eksponerer en extension-metode (`AddPersistence`, `AddRabbitMqMessaging`, `AddFakeMessaging`), så `Program.cs` forbliver tyndt.

```csharp
// Application/Ports
public interface IPositionSource  { IAsyncEnumerable<PositionReceived> ReadAsync(CancellationToken ct); }
public interface IHeatmapNotifier { Task PushAsync(HeatmapSnapshot s, CancellationToken ct); }
public interface IEventPublisher  { Task PublishAsync(EventChanged e, CancellationToken ct); }

// Api/Program.cs  (composition root)
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

if (builder.Environment.IsEnvironment("Local"))
    builder.Services.AddFakeMessaging();      // genererer syntetiske positioner
else
    builder.Services.AddRabbitMqMessaging(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IHeatmapNotifier, SignalRHeatmapNotifier>();
```

Dashboardets komponenter afhænger af fx `IHeatmapFeed`, med en SignalR-implementering og en fake, der genererer tilfældige data, så UI kan udvikles uden backend.

Arkitekturtests kan tilføjes med NetArchTest (C#) og ArchUnit (Java).

---

## 7. Mobile-app

- Oprettes med Vite (vanilla template) i `mobile-app/`.
- `api/positionClient.js` har en fetch-implementering og en fake; `geo/locationProvider.js` wrapper Geolocation API, så positioner kan fakes.
- Build-output (`mobile-app/dist`) kopieres ind i `ingestion-server/bootstrap/src/main/resources/static` (fx via en Maven-plugin eller et script).

---

## 8. Root `.gitignore`

```
.idea/
*.iml
target/
bin/
obj/
node_modules/
dist/
.env
```

Git-repoet skal ligge i `CrowdControl/` (roden), ikke inde i `ingestion-server/`. Den `.gitignore`, Initializr lavede i `ingestion-server/`, må gerne blive liggende.

Ved CI: brug path-filtre, så ændringer i `processing-server/**` kun bygger C#-delen, og `ingestion-server/**` kun Java-delen.

---

## 9. Status og næste skridt

**Gjort**

- [x] Rodprojekt `CrowdControl/` oprettet i IntelliJ som Empty Project med git.
- [x] `ingestion-server/` genereret med Spring Initializr (Spring Boot 4.1.1, Maven, Java 21, groupId `dk.crowdcontrol`) med starters: amqp, flyway, jdbc, validation, webmvc, flyway-database-postgresql, postgresql + tilhørende test-starters.

- [x] `ingestion-server/pom.xml` omskrevet til parent-pom (afsnit 5.1); de ni moduler oprettet med dependencies jf. afsnit 5.2.
- [x] `IngestionServerApplication` flyttet til `bootstrap/` i pakken `dk.crowdcontrol.ingestion`; tomme pakker lagt jf. afsnit 5.3.
- [x] `local`-profil (`bootstrap/src/main/resources/application-local.properties`) slår DataSource-, JDBC-, Flyway- og Rabbit-autokonfiguration fra. `contextLoads()` kører med `@ActiveProfiles("local")`.
- [x] `.\mvnw.cmd clean verify` er grøn. Root `.gitignore` er oprettet, og første commit er lavet på `main`.

**Næste skridt (Java)**

- [ ] Domæne (`Position`, `ActiveEvent`, `EventArea`, `GeoPoint`) og porte/services jf. afsnit 5.4–5.5.
- [ ] In-memory-fakes af alle out-porte i `adapter-in-memory` (`@Profile("local")`).
- [ ] Reload Maven i IntelliJ, så modulerne bliver genkendt.

**Senere**

- [ ] Skriv kontrakter i `contracts/` (OpenAPI, AsyncAPI, JSON-schemas) før implementering.
- [ ] `docker-compose.yml` med RabbitMQ, main-db (PostgreSQL) og event-db (PostgreSQL + PostGIS).
- [ ] Opret `processing-server/` (afsnit 6) og `mobile-app/` (afsnit 7).

---

## 10. Regler for Claude Code i dette repo

- Tilføj **aldrig** Spring-, JDBC-, RabbitMQ- eller andre framework-afhængigheder til `domain` eller `application` (Java) / `Domain` eller `Application` (C#).
- Nye integrationer bliver en ny port i `application` og en adapter i sit eget modul – ikke kode direkte i en service.
- Services i `application` skal ikke have Spring-annotationer; de wires i `bootstrap` (Java) eller `Program.cs` via extension-metoder (C#).
- Hver ny out-port skal have en in-memory/fake-implementering i `adapter-in-memory` (Java) eller en fake i C#, så alt kan køre lokalt uden eksterne systemer.
- Ændringer i kommunikationen mellem containere starter i `contracts/`.
- Java og C# deler aldrig database; migrationer ligger hos den service, der ejer databasen.
- Kør `.\mvnw.cmd clean verify` (Java) / `dotnet build` + `dotnet test` (C#) efter ændringer.
