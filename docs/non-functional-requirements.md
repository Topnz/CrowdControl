# Ikke-funktionelle krav – CrowdControl (FURPS+)

Kravene er afledt af arkitekturen (C2, `CLAUDE.md`) og user stories i `docs/user-stories.md` / Jira. Tal markeret med **(forslag)** er ikke aftalt endnu og bør bekræftes af holdet, før de bruges som acceptkriterier.

**FURPS+**

- **F** – Functionality (tværgående: sikkerhed, privatliv, logning)
- **U** – Usability
- **R** – Reliability
- **P** – Performance
- **S** – Supportability
- **+** – Design-, implementerings-, interface-, fysiske og juridiske begrænsninger

---

## F – Functionality (sikkerhed og privatliv)

| ID | Krav | Måling / verifikation | Relation |
|---|---|---|---|
| NFR-F1 | Deltagere er anonyme: der sendes og gemmes kun koordinater, nøjagtighed, tidspunkt og et tilfældigt session-ID. Ingen navne, IP-adresser eller device-ID'er gemmes. | Gennemgang af `contracts/` og databaseskemaer; test af at IP-adresser ikke logges sammen med positioner. | ID-16, ID-40 |
| NFR-F2 | Rå positioner persisteres ikke. Processing-server gemmer kun aggregerede grid-celler og zonetal. | Skemagennemgang af Main Database og Event-database. | ID-39, arkitektur |
| NFR-F3 | Al trafik mellem browser og servere sker over HTTPS. | Konfigurationsgennemgang; HTTP omdirigeres til HTTPS i produktion. | C2 |
| NFR-F4 | Dashboard og API kræver login. Kun `POST /positions`, event-indstillinger til mobil-appen og statisk indhold er åbne. | Integrationstest: kald uden token giver HTTP 401. | ID-19 |
| NFR-F5 | Kodeord gemmes kun hashet med en anerkendt algoritme (fx ASP.NET Core `PasswordHasher` / PBKDF2). Det udleverede kodeord ligger ikke i repoet. | Kodegennemgang; seed læser kodeord fra konfiguration. | ID-19, ID-30, ID-31 |
| NFR-F6 | Kodeord skal være mindst 10 tegn **(forslag)**. | Unit-test af validering ved kodeordsskift. | ID-30, ID-31 |
| NFR-F7 | Én enhed (session-ID) kan højst sende én position pr. eventets sendeinterval; overskridelser afvises med HTTP 429. | Unit-test af `RateLimiter`. | ID-28, ID-29 |
| NFR-F8 | Interne endpoints mellem servere (`GET /events/active`) og RabbitMQ kræver legitimation, der ikke er committet i repoet. | Konfigurationsgennemgang; `.env` er git-ignoreret. | C2 |

## U – Usability

| ID | Krav | Måling / verifikation | Relation |
|---|---|---|---|
| NFR-U1 | Mobil-appen kan bruges på en smartphone-skærm (fra 360 px bredde) uden zoom og uden at installere noget. | Manuel test på Android (Chrome) og iOS (Safari). | ID-14 |
| NFR-U2 | En deltager kan give samtykke med ét tryk, og samtykketeksten kan læses på under 30 sekunder **(forslag)**. | Brugertest med 3–5 personer. | ID-14, ID-15 |
| NFR-U3 | Delingsstatus ("Deler position" / "Deler ikke") er synlig hele tiden og opdateres inden for 2 sekunder efter ændring. | Manuel test. | ID-22 |
| NFR-U4 | Brugerflader er på dansk. | Gennemgang. | – |
| NFR-U5 | Zoner, der når 90 % af kapaciteten, og zoner over kapacitet markeres med både farve og tekst/ikon, så det ikke kun afhænger af farvesyn. | UI-gennemgang. | ID-27 |
| NFR-U6 | En ny arrangør kan oprette et event med område og mindst én zone på under 5 minutter uden vejledning **(forslag)**. | Brugertest. | ID-13, ID-25 |
| NFR-U7 | Dashboardet virker i de to nyeste versioner af Chrome, Edge og Firefox på desktop. | Manuel test. | – |

## R – Reliability

| ID | Krav | Måling / verifikation | Relation |
|---|---|---|---|
| NFR-R1 | Systemet er tilgængeligt i 99,5 % af et events aktive tidsrum **(forslag)**. | Overvågning af health-endpoints under test-events. | ID-17 |
| NFR-R2 | Ingestion-server kan fortsat modtage og validere positioner, selvom processing-server er nede; positioner bufres i RabbitMQ (durable kø, persistente beskeder) og behandles, når processing-server er oppe igen. | Integrationstest: stop processing-server, send positioner, start igen, tjek at de aggregeres. | C2 |
| NFR-R3 | Ingestion-server henter alle aktive events ved opstart og opdateres via `event-changes`, så den kan genstartes uden manuelle trin. | Integrationstest. | ID-23 |
| NFR-R4 | Mobil-appen håndterer midlertidigt tab af netværk eller GPS uden at gå ned; status viser fejlen, og delingen fortsætter, når forbindelsen er tilbage. | Manuel test i flytilstand. | ID-22 |
| NFR-R5 | Dashboardets live-forbindelse (SignalR) genopretter automatisk forbindelsen inden for 10 sekunder efter et afbrud. | Manuel test: genstart processing-server under live-visning. | ID-17 |
| NFR-R6 | Ugyldige eller ufuldstændige positioner afvises med HTTP 400 og påvirker ikke andre positioner. | Unit- og integrationstest. | ID-16 |

## P – Performance

| ID | Krav | Måling / verifikation | Relation |
|---|---|---|---|
| NFR-P1 | Systemet kan håndtere 10.000 samtidige deltagere i ét event med et sendeinterval på 30 sekunder (≈ 333 positioner/s) **(forslag)**. | Loadtest af `POST /positions` og aggregering. | ID-16, ID-29 |
| NFR-P2 | `POST /positions` svarer på under 200 ms (p95) ved belastningen i NFR-P1. | Loadtest. | ID-16 |
| NFR-P3 | En position er synlig i dashboardets heatmap senest 10 sekunder efter, at den er sendt (p95). | Ende-til-ende-test med tidsstempler. | ID-17 |
| NFR-P4 | Et oprettet eller ændret event modtager positioner inden for 30 sekunder. | Integrationstest. | ID-23 |
| NFR-P5 | En ny eller ændret zone vises med antal deltagere i overblikket inden for 5 sekunder **(forslag)**. | Integrationstest. | ID-26, ID-34 |
| NFR-P6 | Notifikation ved 90 % af en zones kapacitet vises senest 10 sekunder efter, at grænsen er nået. | Test med syntetiske positioner. | ID-27 |
| NFR-P7 | Historik for et event på 8 timer kan hentes og vises på under 3 sekunder **(forslag)**. | Måling mod testdata. | ID-37 |
| NFR-P8 | Mobil-appen må ikke dræne batteriet mærkbart; med et sendeinterval på 30 sekunder bruger appen højst 5 % batteri i timen **(forslag)**. | Manuel måling på testtelefon. | ID-29 |

## S – Supportability

| ID | Krav | Måling / verifikation | Relation |
|---|---|---|---|
| NFR-S1 | Afhængighedsreglen (domain ← application ← adapters ← bootstrap/Api) håndhæves af byggesystemet; `domain`/`application` har ingen framework-afhængigheder. | Maven-moduler / .NET project references; evt. ArchUnit og NetArchTest. | CLAUDE.md §2 |
| NFR-S2 | Hver out-port har en in-memory/fake-implementering, så begge servere kan køre lokalt uden RabbitMQ, databaser eller den anden server. | Java: `--spring.profiles.active=local`; C#: launch-profil `local`. | CLAUDE.md §10 |
| NFR-S3 | Al kommunikation mellem containere er beskrevet i `contracts/` (OpenAPI, AsyncAPI, JSON Schema), og ændringer starter dér. | Kodegennemgang ved pull requests. | CLAUDE.md §10 |
| NFR-S4 | CI bygger og tester Java- og C#-delen ved hver push (path-filtre); `main` skal altid være grøn. | GitHub Actions. | CLAUDE.md §8 |
| NFR-S5 | Domain- og application-lagene har mindst 80 % testdækning **(forslag)**. | Dækningsrapport (JaCoCo / coverlet). | – |
| NFR-S6 | Hele miljøet (RabbitMQ og databaser) kan startes med `docker compose up` på under 5 minutter på en ny udviklermaskine. | Manuel test. | CLAUDE.md §9 |
| NFR-S7 | Databaseskemaer ændres kun via migrationer (Flyway for Event-database, EF Core for Main Database), som køres automatisk ved opstart. | Kodegennemgang. | CLAUDE.md §2 |
| NFR-S8 | Begge servere logger struktureret (niveau, tidspunkt, korrelations-ID) uden persondata, og har et health-endpoint. | Kodegennemgang; `/actuator/health` og `/health`. | NFR-F1 |

## + Design-, implementerings-, interface- og juridiske krav

| ID | Type | Krav | Relation |
|---|---|---|---|
| NFR-D1 | Design | Hexagonal arkitektur (Ports & Adapters) i begge servere; hver adapter er sit eget modul/projekt. | CLAUDE.md §2 |
| NFR-D2 | Design | Hver server ejer sin egen database; Java og C# deler aldrig database. | CLAUDE.md §2 |
| NFR-D3 | Design | Systemet er single-tenant: én installation pr. arrangør med én præoprettet konto. | ID-19 |
| NFR-I1 | Implementering | Ingestion-server: Java 21, Spring Boot 4.1, Maven multi-module. | CLAUDE.md §5 |
| NFR-I2 | Implementering | Processing-server og dashboard: C#/.NET 10, ASP.NET Core, EF Core, SignalR, Blazor WebAssembly. | CLAUDE.md §6 |
| NFR-I3 | Implementering | Mobil-appen er ren HTML/JavaScript uden frameworks, CSS-frameworks eller build-værktøj, så den kan embeddes i et andet system. | CLAUDE.md §7 |
| NFR-I4 | Implementering | Databaser: PostgreSQL med PostGIS (Event-database) og PostgreSQL (Main Database); message broker: RabbitMQ. | C2 |
| NFR-IF1 | Interface | Mobil-app → ingestion: REST/JSON over HTTPS efter `contracts/openapi/ingestion-api.yaml`. | C2 |
| NFR-IF2 | Interface | Java ↔ C#: AMQP efter `contracts/asyncapi/messaging.yaml` og versionerede JSON Schemas (`*.v1.json`); brudændringer kræver ny version. | C2 |
| NFR-IF3 | Interface | Processing-server → dashboard: SignalR/WebSocket til live-data, REST/JSON til historik og administration. | C2 |
| NFR-L1 | Juridisk | Behandling af positionsdata overholder GDPR: informeret, aktivt samtykke, som kan trækkes tilbage når som helst. | ID-14, ID-15, ID-18 |
| NFR-L2 | Juridisk | Aggregerede data slettes automatisk efter 24 måneder (dataminimering og opbevaringsbegrænsning). | ID-39 |
| NFR-L3 | Juridisk | Positionsdeling stopper automatisk ved eventets sluttid, så der ikke indsamles data ud over formålet. | ID-21 |
| NFR-PH1 | Fysisk | Deltagerens enhed skal have en browser med Geolocation API og GPS; positionens nøjagtighed afhænger af enheden og rapporteres i meter. | ID-40 |
