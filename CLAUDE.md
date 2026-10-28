# CrowdControl – projektkontekst

Denne fil samler arkitektur, beslutninger og opsætning for CrowdControl-projektet (SEP3). Den er skrevet til at ligge i repo-roden som `CLAUDE.md`, så Claude Code automatisk læser den.

> **⚠ ÅBEN TODO – Claude skal nævne dette i sit første svar i hver session, indtil punktet er afkrydset:**
> Dashboardet skal flyttes ud af `processing-server/` til sin egen mappe/solution `dashboard/` på niveau med `mobile-app/`. Den fulde plan står i **afsnit 9.1**. Fjern denne boks, når 9.1 er gennemført.

---

## 1. Systemet (fra C2-diagrammet)

CrowdControl viser et live heatmap over, hvor deltagere befinder sig til et event (fx en festival).

**Personer**

- **Event-Participant** – deltager i eventet, giver samtykke og deler sin position via mobil-appen.
- **Event Holder** – medarbejder hos arrangøren, der overvåger eventet og opretter events/zoner i dashboardet.

**Én arrangør pr. system (single-tenant).** Hver installation tilhører én arrangør. Der er ingen kunder/organisationer og ingen brugeradministration i systemet: udviklerne opretter arrangørens konto (seedet via EF Core-migration med hashet kodeord), og arrangøren skal ændre kodeordet ved første login.

**Containere**

| Container | Teknologi | Ansvar |
|---|---|---|
| Mobile app | JavaScript, HTML (mobilbrowser) | Læser position via Geolocation API i et givent interval og sender den pseudonymt. Serveres som statisk indhold af ingestion-server. |
| Ingestion-server | Java, Spring Boot | Modtager positioner, validerer dem mod aktive events og eventområder, rate-limiter og publicerer til køen. Serverer mobil-appen. |
| Event-database | PostgreSQL + PostGIS | Kopi af aktive events: ID, tidsrum og område. Ejes af ingestion-server. |
| Message broker | RabbitMQ | Kø med positioner (Java → C#) og exchange med event-ændringer (C# → Java). |
| Processing-server | C#, ASP.NET Core | Login, events, aggregering til grid-celler, historik, zoner. Serverer dashboardet. |
| Main Database | PostgreSQL | Tabeller: users (arrangørens konto), events, heatmap, zones. Ejes af processing-server. |
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
| Processing-server → Main Database | Læser/skriver bruger, events, zoner, heatmap | EF Core, SQL |
| Processing-server → Dashboard | Pusher live-opdateringer | SignalR, WebSocket |
| Dashboard → Processing-server | Henter historik | REST, JSON/HTTPS |
| Dashboard → Processing-server | Henter statisk indhold | HTTPS |

**Aktører**

- **Arrangør** – står for at afholde et event og vil have visuelle oversigter over, hvor deltagerne befinder sig på bestemte tidspunkter.
- **Deltager** – deltager i et event og ønsker den bedst mulige oplevelse ved eventet.

**Epics** (Jira-projekt `ID`; stories, acceptkriterier og use case-resuméer i `docs/architecture/analyse/krav/user-stories.md`. I user stories hedder Event Holder "Arrangør" og Event-Participant "Deltager")

| Epic | Stories | Indhold |
|---|---|---|
| ID-5 Håndtér samtykke & privatliv | ID-14, 15, 18, 21, 22 | Aktivt samtykke; information om indsamlede data og opbevaringstid før samtykke; stop deling; automatisk stop når eventet slutter; vis om positionen deles lige nu. |
| ID-6 Del position | ID-16, 20, 28, 29, 40 | Pseudonym deling; antal deltagere vises kun for eventets zoner; arrangøren bestemmer sendeintervallet; rate limiting pr. enhed; nøjagtighed i meter på hver position, så upræcise data kan frasorteres. |
| ID-7 Administrér event | ID-13, 23, 24, 33, 35 | Opret/redigér/slet event; liste over kommende og aktive events (med navn, tidsrum, zoner og status); et oprettet/redigeret event modtager positioner inden for 30 sekunder. |
| ID-8 Administrér zone | ID-25, 26, 27, 34, 36 | Opret/redigér/slet zoner; maks. antal deltagere pr. zone med notifikation, når antallet når 90 % af kapaciteten, og markering ved overskridelse; ny zone vises straks med antal deltagere. |
| ID-9 Overvåg event | ID-17, 41 | Live heatmap (positioner samlet i grid-celler) og antal deltagere pr. zone, opdateret ved hver ny data uden genindlæsning; positioner uden for eventets område og tidsrum afvises af ingestion-server. |
| ID-10 Se historik | ID-37, 38, 39 | Liste over afholdte events (navn, tidsrum, område på kort); afspil heatmap og zonetal for et valgt tidsinterval; automatisk sletning af aggregerede data efter 24 måneder. |
| ID-11 Log ind & administrér konto | ID-19, 30, 31, 32 | Log ind (præoprettet konto, ingen adgang til dashboard eller API uden login); skift kodeord ved første login; ændre kodeord (kræver nuværende kodeord); log ud. |

Konsekvenser for arkitekturen:

- Ingestion-server afviser positioner uden for et aktivt events område og tidsrum, så de aldrig sendes til processing-server. Zoner oprettes i processing-server og kopieres til ingestion-server (med eventet), som slår op, hvilke zoner en position ligger i, og sender `sessionId`, position, `eventId` og zone-ID'er videre. Kapacitet og optælling håndteres kun i processing-server.
- Sendeinterval og eventets sluttid skal kunne hentes af mobil-appen (via ingestion-server), da appen skal sende i det valgte interval og stoppe automatisk. Det kræver felter i `contracts/`.
- Hver position har nøjagtighed i meter (`accuracy` fra Geolocation API), som sendes med hele vejen til processing-server, så upræcise positioner kan frasorteres før zone-optælling (ID-40).
- Rå positioner persisteres ikke; kun aggregerede grid-celler gemmes, og de slettes automatisk efter 24 måneder (fast opbevaringstid, ikke konfigurerbar). Opbevaringstiden står som fast tekst på samtykkesiden.

### 1.1 Begreber

| Begreb | Betydning |
|---|---|
| Event | Et arrangement med navn, start- og sluttid, et område tegnet som polygon på et kort og et sendeinterval. Status: kommende, aktiv eller afsluttet. |
| Zone | Et navngivent delområde inden for et events område (fx "Bøgescenen Sektion A") med en valgfri kapacitet (maks. antal deltagere). Arrangøren får en notifikation, når antallet når 90 % af kapaciteten, og zonen markeres, når kapaciteten overskrides. |
| Sendeinterval | Hvor ofte mobil-appen sender deltagerens position. Sættes af arrangøren pr. event. |
| Samtykke | Deltagerens aktive accept af positionsdeling. Kan trækkes tilbage med stop-knappen (session-ID'et slettes i browseren) og ophører automatisk ved eventets sluttid. |
| Position | Koordinater, nøjagtighed i meter og tidspunkt, sendt pseudonymt med et tilfældigt session-ID. Aggregerede data er anonyme. |
| Historik | Aggregerede positionsdata (antal deltagere i eventet og zonerne over tid), som slettes efter 24 måneder. |

### 1.2 Use cases

Én use case pr. epic (UC1–UC7, resuméer og stories i `docs/architecture/analyse/krav/user-stories.md`):

- **Deltager:** UC1 Håndtér samtykke & privatliv, UC2 Del position (primær).
- **Arrangør:** UC2 Del position (sekundær), UC3 Administrér event, UC4 Administrér zone, UC5 Overvåg event, UC6 Se historik (primær), UC7 Log ind & administrér konto.
- **System:** UC6 Se historik (automatisk sletning efter 24 måneder).

**UC3 Administrér event (ID-7)**, fully dressed (påbegyndt):

- *Aktør:* Arrangør.
- *Basis-sekvens:* Systemet viser en liste over kommende og aktive events samt "Opret nyt event". Arrangøren vælger at oprette et nyt event eller åbne et eksisterende.
- *Scenarie A, opret nyt event:*
  1. Systemet viser felter til navn, start- og sluttid.
  2. Arrangøren udfylder felterne.
  3. Systemet viser et kort.
  4. Arrangøren tegner eventets område som en polygon ved at vælge hjørnerne.
  5. Systemet gemmer eventet.

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
│   ├── analyse/
│   │   ├── krav/                    # user-stories.md, non-functional-requirements.md
│   │   └── ssd/                     # systemsekvensdiagrammer
│   └── design/
│       ├── c2/                      # c2.drawio + c2.png
│       ├── klassediagram/
│       ├── sekvensdiagrammer/
│       └── kommunikationsprotokol/  # HTTP- og AMQP-protokoller (PlantUML)
│
├── contracts/                       # Det ENESTE der deles på tværs af containere
│   ├── openapi/
│   │   ├── ingestion-api.yaml       # POST /positions            (mobil → Java)
│   │   └── processing-api.yaml      # GET /events/active, /history (→ C#)
│   ├── asyncapi/
│   │   └── messaging.yaml           # positions-kø + event-exchange
│   └── schemas/
│       ├── rabbitMQ/                # beskeder på køen/exchangen
│       │   ├── PositionReceived.v1.json
│       │   ├── EventCreated.v1.json
│       │   ├── EventUpdated.v1.json
│       │   └── EventDeleted.v1.json
│       └── shared/
│           └── common.v1.json       # GeoPoint, Polygon, Zone, Event (AMQP + REST)
│
├── mobile-app/                      # ren HTML/JS, intet build-værktøj, CSS kun inline
│   ├── index.html                   # samtykke-knap
│   └── app.js                       # Geolocation API → POST /positions
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
│                                    # pom'en pakker mobile-app/ som static/
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
- Udviklingsmiljøet er Windows. Maven køres med `.\mvnw.cmd` fra `ingestion-server/`.

### 4.1 Kørsel (fælles run-konfigurationer)

Alle har de samme konfigurationer ved den grønne startknap. De ligger i git og må ikke laves lokalt i stedet (detaljer og fejlfinding i `README.md`).

| Tilstand | Ingestion (Spring-profil) | Api (launch-profil → miljø) | Docker compose |
|---|---|---|---|
| `offline` | `offline`: in-memory fakes, ingen RabbitMQ/PostGIS | `offline` → `Offline`: ingen RabbitMQ (main-db bruges stadig til migrationer) | Nej |
| `lokal` | `lokal`: rigtig infrastruktur, binder kun til `127.0.0.1` | `lokal` → `Development`, `localhost:5262` | Ja, startes før |
| `LAN` | `lan`: starter en Cloudflare quick tunnel (`cloudflared`) og printer et offentligt HTTPS-link. HTTPS er krævet af Geolocation API | `lan` → `Development`, `0.0.0.0:5262`, printer et link pr. LAN-IP | Ja, startes før |

- **IntelliJ:** `.run/` i roden (`Infrastructure (docker compose)`, `Ingestion – offline/lokal/LAN` som Maven-konfigurationer med `-pl bootstrap -am spring-boot:run`; `-am`, så søskendemoduler bygges med i stedet for at blive hentet fra `~/.m2`).
- **Rider:** `processing-server/.run/` (`Infrastructure (docker compose)` med `../docker-compose.yml`, `Api – offline/lokal/LAN`, `Dashboard – offline/lokal/LAN` og compounds `Api + Dashboard – …`). Rider ser kun `.run/` ved siden af solutionen, så docker-konfigurationen findes i begge `.run/`-mapper. Api'ets og Dashboardets `launchSettings.json` indeholder kun de tre profiler (Dashboard på port 5111); der må ikke tilføjes `http`/`https`-profiler.
- **VS Code:** `.vscode/launch.json` + `tasks.json` (de eneste filer i `.vscode/`, der er i git). Compounds `Alt – lokal` og `Alt – LAN` starter ingestion, Api og dashboard sammen. De bygger Api'et én gang først og starter Api og dashboard med `--no-build`, da Api'et refererer Dashboard-projektet, og to samtidige builds låser `obj/`.
- **Dashboard alene** (`Dashboard – …`, port 5111) kalder Api'et via `ApiBaseUrl` i `wwwroot/appsettings.json` (`http://{host}:5262`, `{host}` = det værtsnavn, dashboardet er åbnet på). Api'et tillader dashboardets origin via CORS (`Dashboard:Origins`; alle origins i Development/Offline).
- Links printes ved opstart af `bootstrap/…/startup/` (Java) og `Api/StartupLinks.cs` (C#). Nye applikationer skal også printe deres link.

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
@Component @Profile("!offline")
class RabbitPositionPublisher implements PositionPublisher { ... }

// adapter-in-memory
@Component @Profile("offline")
class LoggingPositionPublisher implements PositionPublisher { ... }
```

Med `--spring.profiles.active=offline` skal Java-serveren kunne køre helt uden RabbitMQ, PostGIS og C#-serveren. Bemærk: `offline`-profilen skal også slå DataSource- og Flyway-autokonfiguration fra (fx via `spring.autoconfigure.exclude` i `application-offline.properties`), ellers forsøger Boot at forbinde til databasen ved opstart. Tjek de præcise klassenavne for Spring Boot 4, da autokonfigurationerne er flyttet til nye pakker.

---

## 6. Processing-server (C#) – .NET solution

### 6.1 Oprettelse med `dotnet` CLI

Kør fra `CrowdControl/processing-server`:

```bash
dotnet new sln -n CrowdControl.Processing --format sln

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

if (builder.Environment.IsEnvironment("Offline"))
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

- "Appen" er udelukkende et sted, hvor brugeren accepterer at dele sin geolocation. Den skal på sigt embeddes i et andet system.
- Kun `index.html` (information om indsamlede data og opbevaringstid på 24 måneder, samtykke-knap, stop-knap, status for deling) og `app.js` (Geolocation API → `POST /positions` i det interval, arrangøren har valgt, med et pseudonymt `sessionId`; stopper automatisk ved eventets sluttid). CSS står i en `<style>`-blok i `index.html` (ingen separate CSS-filer). Ingen frameworks, intet build-værktøj (ingen npm/Vite).
- `ingestion-server/bootstrap/pom.xml` kopierer `mobile-app/` til `static/` med `maven-resources-plugin:copy-resources` (fase `process-resources`), så siden serveres af ingestion-server uden et manuelt kopi-trin. Bevidst ikke som `<resource>`, da IntelliJ så viser mappen som `mobile-app [bootstrap]`. Kør derfor ingestion-server via Maven (`spring-boot:run`) eller med "Delegate IDE build/run actions to Maven" slået til, hvis mobil-appen skal serveres.
- Payloaden i `app.js` skal følge `contracts/openapi/ingestion-api.yaml`.

---

## 8. Root `.gitignore`

```
.idea/  *.iml  .vs/  .vscode/*  *.user  *.DotSettings.user
!.vscode/launch.json  !.vscode/tasks.json  !.vscode/extensions.json
target/  bin/  obj/  TestResults/  node_modules/
.env
```

`.run/` (begge steder) og de tre `.vscode/`-filer skal være i git (afsnit 4.1).

Bemærk: `dist/` må **ikke** ignoreres, da Dashboardet bruger `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css` (kun den fil er beholdt fra Blazor-skabelonen).

Git-repoet skal ligge i `CrowdControl/` (roden), ikke inde i `ingestion-server/`. Den `.gitignore`, Initializr lavede i `ingestion-server/`, må gerne blive liggende.

Ved CI: brug path-filtre, så ændringer i `processing-server/**` kun bygger C#-delen, og `ingestion-server/**` kun Java-delen.

---

## 9. Status og næste skridt

**Gjort**

- [x] Rodprojekt `CrowdControl/` oprettet i IntelliJ som Empty Project med git.
- [x] `ingestion-server/` genereret med Spring Initializr (Spring Boot 4.1.1, Maven, Java 21, groupId `dk.crowdcontrol`) med starters: amqp, flyway, jdbc, validation, webmvc, flyway-database-postgresql, postgresql + tilhørende test-starters.

- [x] `ingestion-server/pom.xml` omskrevet til parent-pom (afsnit 5.1); de ni moduler oprettet med dependencies jf. afsnit 5.2.
- [x] `IngestionServerApplication` flyttet til `bootstrap/` i pakken `dk.crowdcontrol.ingestion`; tomme pakker lagt jf. afsnit 5.3.
- [x] `offline`-profil (`bootstrap/src/main/resources/application-offline.properties`, hed tidligere `local`) slår DataSource-, JDBC-, Flyway- og Rabbit-autokonfiguration fra. `contextLoads()` kører med `@ActiveProfiles("offline")`.
- [x] `.\mvnw.cmd clean verify` er grøn. Root `.gitignore` er oprettet, og første commit er lavet på `main`.
- [x] `processing-server/` oprettet jf. afsnit 6 (`dotnet new sln --format sln`, da .NET 10 ellers laver `.slnx`). `Api` serverer Dashboard; launch-profilen `offline` sætter miljøet `Offline` (hed tidligere `local`/`Local`). `dotnet build` + `dotnet test` er grønne.
- [x] `contracts/` med skabeloner: OpenAPI (`POST /positions`, `GET /events/active`, `GET /history`), AsyncAPI (`positions`-kø, `event-changes`-exchange) og JSON-schemas uden felter.
- [x] `docker-compose.yml`: RabbitMQ (5672, UI 15672), main-db (5432, `maindb`), event-db PostGIS (5433, `eventdb`). Bruger/password: `crowdcontrol`. Forbindelserne står i Java `application.properties` og C# `appsettings.json`.
- [x] `mobile-app/` (afsnit 7).
- [x] EF Core i `Infrastructure.Persistence`: Npgsql-provider, snake_case-navne, tom `MainDbContext`, `AddPersistence(IConfiguration)` og `Configurations/` til `IEntityTypeConfiguration<T>`. `Api` kører `Database.Migrate()` ved opstart i Development/Offline. `dotnet-ef` er lokalt værktøj (`processing-server/dotnet-tools.json`; kør `dotnet tool restore`). Ny migration: `dotnet dotnet-ef migrations add <Navn> --project src/CrowdControl.Infrastructure.Persistence --startup-project src/CrowdControl.Api`.
- [x] CI: `.github/workflows/ingestion-server.yml` og `processing-server.yml` (GitHub Actions) med path-filtre; `contracts/**` trigger begge. `mvnw` er markeret eksekverbar i git.
- [x] Fælles run-konfigurationer for IntelliJ, Rider og VS Code med tilstandene `offline`, `lokal` og `LAN` (afsnit 4.1). Spring-profilen `local` og C#-miljøet `Local` er omdøbt til `offline`/`Offline`. Kørselsvejledning i `README.md`.

**Næste skridt**

- [ ] Aftal felterne i `contracts/` (schemas og request/response-typer), inkl. sendeinterval og sluttid til mobil-appen.
- [ ] Gør acceptkriterierne i `docs/architecture/analyse/krav/user-stories.md` målbare (SMART), fx tærsklen for upræcise positioner (ID-40).
- [ ] Eksportér `docs/architecture/design/c2/c2.png` igen fra den opdaterede `c2.drawio`.
- [ ] Seed arrangørkontoen via EF Core-migration (hashet kodeord fra konfiguration, ikke i repoet; flag for skift ved første login).
- [ ] Reload Maven i IntelliJ og åbn `CrowdControl.Processing.sln` i Rider.
- [ ] **Flyt dashboardet til egen mappe/solution (`dashboard/`), se afsnit 9.1.**

### 9.1 TODO: Flyt dashboardet ud af processing-server

**Hvorfor.** Dashboardet er sin egen container i C2-diagrammet, men ligger i `CrowdControl.Processing.sln` og serveres af `Api`. Kontrakten mellem dashboard og Api er i dag C#-projektet `CrowdControl.Shared` (`CellDto`, `HeatmapSnapshotDto`, `ZoneCountDto`) og ikke `contracts/`. Det bryder reglen om, at kommunikation mellem containere starter i `contracts/` (afsnit 10). Når dashboardet flyttes ud, bliver REST- og SignalR-kontrakten eksplicit på samme måde som mellem Java og C#.

**Beslutninger (truffet):**

- **Intet domænelag i dashboardet.** Dashboardet er en klient. Forretningsregler (grid-aggregering, kapacitet, 90 %-tærskel, validering) bliver i processing-server, så der kun er én kilde til sandheden. Dashboardet får i stedet ports & adapters i lille skala: DTO'er, interfaces, UI-tilstand og fakes.
- **Hosting: dashboardet bliver et selvstændigt statisk site** (egen dev-server lokalt, fx nginx i docker-compose senere). `Api` serverer **ikke** længere dashboardet og tilbyder kun REST + SignalR. Konsekvenser: CORS i `Api`, og login (ID-30) skal være token-baseret (JWT bearer). SignalR sender tokenet via `accessTokenFactory`. Alternativet (Api serverer det publicerede output via et kopi-trin, som Maven gør med `mobile-app/`) er fravalgt, fordi det kobler buildet sammen igen.
- **`CrowdControl.Shared` nedlægges** (se nedenfor).

**Plan for `CrowdControl.Shared`:**

1. Beskriv de tre DTO'er som JSON-schemas i `contracts/` **før** noget flyttes: `contracts/schemas/signalr/HeatmapSnapshot.v1.json` (med `Cell` og `ZoneCount`), alternativt som en `dashboard`-kanal i `contracts/asyncapi/messaging.yaml`. REST-endpoints (`/history`, senere events/zoner/login/skift kodeord) beskrives i `contracts/openapi/processing-api.yaml`.
2. **Application må ikke afhænge af DTO'er.** I dag refererer `CrowdControl.Application` til `Shared`, og `GridAggregator` bygger og returnerer `HeatmapSnapshotDto`/`CellDto`/`ZoneCountDto` (testene i `GridAggregatorTests` asserter på dem). Da `Application` ikke må referere `Api` (afhængighedsreglen), skal `GridAggregator` i stedet returnere egne modeltyper, fx `HeatmapSnapshot`, `GridCell`, `ZoneCount` i `CrowdControl.Application` (eller `Domain`). Det matcher `HeatmapSnapshot` i afsnit 6.3. `GridAggregatorTests` opdateres til de nye typer, og `using CrowdControl.Shared;` fjernes.
3. **Serversiden:** flyt `CellDto`, `HeatmapSnapshotDto` og `ZoneCountDto` ind i `CrowdControl.Api/Contracts/` (namespace `CrowdControl.Api.Contracts`). Det er Api's response-typer og bruges af `HeatmapController`, `HeatmapBroadcaster` og `LatestHeatmap`. Api mapper fra Application-modellen til DTO'en (fx en `ToDto()`-extension i `Api/Contracts/`), før den sendes via SignalR/REST.
4. **Klientsiden:** dashboardet får sine egne kopier i `CrowdControl.Dashboard.Core/Contracts/`, skrevet i hånden mod schemas/OpenAPI. Kodegenerering (NSwag/Kiota) overvejes, når API'et vokser.
5. Slet projektet `CrowdControl.Shared` og fjern det fra `CrowdControl.Processing.sln`. Der må **ikke** være nogen project reference på tværs af `dashboard/` og `processing-server/`. Det er bevidst, at DTO'erne findes to steder: kontrakten i `contracts/` er det, der holder dem i sync.

**Målstruktur:**

```
dashboard/
├── CrowdControl.Dashboard.sln
├── src/
│   ├── CrowdControl.Dashboard.Core/      # ingen Blazor/SignalR-afhængigheder
│   │   ├── Contracts/   # DTO'er jf. contracts/
│   │   ├── Ports/       # IHeatmapFeed, IEventsApi, IZonesApi, IHistoryApi, IAuthApi
│   │   └── State/       # UI-tilstand og ren UI-logik (farveskala, "tæt på kapacitet"-visning, afspilningstidslinje)
│   ├── CrowdControl.Dashboard.Adapters/  # SignalRHeatmapFeed, Http*Api, Fake* (tilfældige data)
│   │                                     # (deles i .SignalR/.Http/.Fakes, hvis det vokser)
│   └── CrowdControl.Dashboard/           # Blazor WASM: Pages, Layout, Components, wwwroot (map.js, lib/bootstrap)
│                                         # Program.cs = composition root (Fake vs. rigtig adapter efter miljø)
└── tests/
    └── CrowdControl.Dashboard.Tests/     # Core-tests + bUnit-komponenttests
```

Afhængighedsreglen: `Core ← Adapters ← Dashboard`, håndhævet med project references. Adapters skal kunne køre i WebAssembly (ingen serverbiblioteker).

**Pakker og referencer, der skal fjernes/flyttes** (status pr. 2026-10-27; tjek csproj-filerne igen før flytningen):

| Projekt | Fjern | Behold / tilføj |
|---|---|---|
| `processing-server/…/CrowdControl.Api.csproj` | `ProjectReference` til `CrowdControl.Dashboard` og `CrowdControl.Shared`; `PackageReference` `Microsoft.AspNetCore.Components.WebAssembly.Server` (bruges kun til at servere WASM) | Behold `Microsoft.EntityFrameworkCore.Design` (dotnet-ef). Tilføj senere `Microsoft.AspNetCore.Authentication.JwtBearer` (ID-30). CORS og SignalR-server er en del af `Microsoft.NET.Sdk.Web`, så de kræver ingen pakke. |
| `processing-server/…/CrowdControl.Application.csproj` | `ProjectReference` til `CrowdControl.Shared` | Kun `Domain` (afhængighedsreglen). |
| `processing-server/…/CrowdControl.Shared` | Hele projektet, også fra `CrowdControl.Processing.sln` | – |
| `processing-server/…/CrowdControl.Dashboard` | Hele projektet fra processing-server (flyttes) | – |
| `processing-server/tests/CrowdControl.Application.Tests` | `using CrowdControl.Shared;` i `GridAggregatorTests.cs` | Refererer kun `Application`. |
| `processing-server/tests/CrowdControl.Api.IntegrationTests` | – (refererer `Api`, som ikke længere trækker Dashboard/Shared med transitivt) | Tjek at den stadig bygger uden de transitive referencer. |
| `dashboard/…/CrowdControl.Dashboard` (WASM) | `Blazor.Bootstrap` 4.0.0. Den bruges ikke i nogen `.razor`-fil; styling sker med den lokale `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css`. Fjern den, hvis den stadig er ubrugt, ellers flyt den med. `ProjectReference` til `Shared`. | `Microsoft.AspNetCore.Components.WebAssembly`, `…WebAssembly.DevServer` (PrivateAssets=all). `ProjectReference` til `Dashboard.Adapters` og `Dashboard.Core`. `Microsoft.AspNetCore.SignalR.Client` flyttes **ikke** hertil, kun til Adapters. |
| `dashboard/…/CrowdControl.Dashboard.Core` | – | **Ingen pakker** (ingen Blazor, SignalR eller Http-klientbiblioteker). Ren `Microsoft.NET.Sdk` classlib. |
| `dashboard/…/CrowdControl.Dashboard.Adapters` | – | `Microsoft.AspNetCore.SignalR.Client` (flyttet fra WASM-projektet), `Microsoft.Extensions.DependencyInjection.Abstractions` (til `AddSignalRAdapters()`/`AddFakeAdapters()`-extensions). `ProjectReference` til `Core`. Evt. `System.Net.Http.Json` er allerede i BCL. |
| `dashboard/tests/CrowdControl.Dashboard.Tests` | – | xUnit-pakkerne som i processing-servers testprojekter (`Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`) + `bunit` til komponenttests. |

Brug de samme versioner som i dag (10.0.13 for `Microsoft.AspNetCore.*`), så de to solutions ikke glider fra hinanden. Overvej en `Directory.Packages.props` (central package management) i hver solution.

Øvrige filer/indstillinger, der hører til dashboardet og skal med eller ryddes op:

- `wwwroot/lib/bootstrap/dist/css/bootstrap.min.css`: flyttes med. `.gitignore`-noten i afsnit 8 (`dist/` må ikke ignoreres) gælder fortsat, men for den nye sti.
- `Properties/launchSettings.json`: dashboardet har profilerne `offline`/`lokal`/`lan` (port 5111) og `Dashboard – …`-konfigurationer (afsnit 4.1). De flyttes med. I `Api`'s `launchSettings.json` skal `inspectUri` (WASM-debugging) fjernes, hvis den findes.
- `.github/workflows/processing-server.yml` bygger hele solutionen og dermed i dag også dashboardet. Efter flytningen bygger den kun serveren; dashboardet får sin egen workflow (trin 8).

**Trin:**

- [ ] 1. Kontrakter: schemas for heatmap-snapshot (SignalR) og `/history` i `contracts/` (jf. ovenfor).
- [ ] 1b. Afkobl `Application` fra `Shared` (`GridAggregator` returnerer egne modeltyper, Api mapper til DTO'er), og kør `dotnet build` + `dotnet test`, **før** dashboardet flyttes. Det kan laves og merges som sit eget lille PR.
- [ ] 2. Opret `dashboard/` med solution, Core, Adapters, WASM-projekt og testprojekt (`dotnet new sln --format sln`, som i afsnit 6.1).
- [ ] 3. Flyt `Pages/`, `Layout/`, `App.razor`, `_Imports.razor` og `wwwroot/` (inkl. `map.js` og `lib/bootstrap/dist/css/bootstrap.min.css`) over. Slet skabelonresten `wwwroot/sample-data/weather.json`.
- [ ] 4. `Services/MapServise.cs` (stavefejl, klassen hedder `MapService`) splittes: `IHeatmapFeed` i Core, `SignalRHeatmapFeed` i Adapters og en `FakeHeatmapFeed`, så UI kan køre uden backend.
- [x] 5. **API-adresse:** `ApiBaseUrl` i `wwwroot/appsettings.json` (`http://{host}:5262`) læses af `Services/ApiAddress.cs`, som HttpClient og hub-URL bygges ud fra. Api'et har en CORS-policy (`Dashboard:Origins`, alle origins i Development/Offline), så den del af trin 6 er også gjort.
- [ ] 6. Processing-server: fjern projekter, pakker og referencer jf. tabellen ovenfor, og fjern `UseStaticWebAssets()` (Offline), `UseWebAssemblyDebugging()`, `UseBlazorFrameworkFiles()`, `UseStaticFiles()` og `MapFallbackToFile("index.html")` fra `Program.cs`. Tilføj en CORS-policy for dashboardets origin (konfigurerbar; SignalR kræver `AllowCredentials` + eksplicit origin).
- [ ] 7. Login (ID-30/31/32), når det implementeres: JWT bearer i Api; dashboardet gemmer tokenet og sender det i `Authorization`-headeren og via `accessTokenFactory` til SignalR. Hub og controllers får `[Authorize]`.
- [ ] 8. CI: ny `.github/workflows/dashboard.yml` med path-filter `dashboard/**` + `contracts/**`. Fjern dashboard-delen fra `processing-server.yml`, hvis den har nogen.
- [ ] 9. Kørsel: `Dashboard – offline/lokal/LAN` findes i Rider (`processing-server/.run/`) og VS Code (inkl. compounds). Mangler ved flytningen: flyt Rider-konfigurationerne til `dashboard/.run/`, opdatér stierne i `.vscode/`, drop `--no-build`-varianterne (ingen delt build længere), lad dashboardet printe sit eget LAN-link (dev-serveren printer kun `0.0.0.0`), og lad Api'et kun printe API-adressen.
- [ ] 10. Docker (valgfrit, senere): `dashboard`-service i `docker-compose.yml`, der serverer `dotnet publish`-output statisk.
- [ ] 11. Opdatér dokumentationen: C2-diagrammet (`c2.drawio` → `c2.png`: dashboardet serveres ikke længere af processing-server), og i denne fil afsnit 1 (container- og kommunikationstabellerne: "Dashboard → Processing-server: Henter statisk indhold" udgår), afsnit 3 (repo-struktur), 6.1–6.3 (Dashboard/Shared udgår; `IHeatmapFeed` flyttes til `dashboard/`), 8 (stien til `bootstrap.min.css`) og 10 (kør også `dotnet build` + `dotnet test` i `dashboard/`). Fjern ⚠-boksen øverst.
- [ ] 12. Verificér: `dotnet build` + `dotnet test` grøn i både `processing-server/` og `dashboard/`; live heatmap virker end-to-end (Api + dashboard) og med fake-feed alene.

---

## 10. Regler for Claude Code i dette repo

- Tilføj **aldrig** Spring-, JDBC-, RabbitMQ- eller andre framework-afhængigheder til `domain` eller `application` (Java) / `Domain` eller `Application` (C#).
- Nye integrationer bliver en ny port i `application` og en adapter i sit eget modul – ikke kode direkte i en service.
- Services i `application` skal ikke have Spring-annotationer; de wires i `bootstrap` (Java) eller `Program.cs` via extension-metoder (C#).
- Hver ny out-port skal have en in-memory/fake-implementering i `adapter-in-memory` (Java) eller en fake i C#, så alt kan køre lokalt uden eksterne systemer.
- Ændringer i kommunikationen mellem containere starter i `contracts/`.
- Java og C# deler aldrig database; migrationer ligger hos den service, der ejer databasen.
- Kommentarer og Javadoc/XML-docs skrives på engelsk (Java, C#, JS, CSS, HTML og konfigurationsfiler). Tekst, brugerne ser (UI, statusbeskeder), forbliver på dansk.
- Kør `.\mvnw.cmd clean verify` (Java) / `dotnet build` + `dotnet test` (C#) efter ændringer.
