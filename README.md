# CrowdControl

Live heatmap over, hvor deltagerne befinder sig til et event. Arkitektur og beslutninger står i [`CLAUDE.md`](CLAUDE.md).

## Kør projektet

Alle har de samme start-konfigurationer i IntelliJ, Rider og VS Code. Vælg dem i dropdownen ved den grønne startknap.

### Tilstande

| Tilstand | Hvad den gør | Kræver |
|---|---|---|
| **offline** | Kører uden RabbitMQ og PostGIS. Ingestion logger positionerne i stedet for at sende dem videre. | Api'et bruger stadig main-db, så start `Infrastructure (docker compose)`, hvis du kører Api'et |
| **lokal** | Hele systemet på din egen maskine. Kun tilgængelig fra `localhost`. | Docker (startes automatisk) |
| **LAN** | Som `lokal`, men andre kan åbne links: mobil-appen via en Cloudflare-tunnel (HTTPS) og dashboardet via din LAN-IP | Docker (startes automatisk) og `cloudflared` |

Mobil-appen bruger Geolocation API, som browsere kun tillader på `https://` eller `localhost`. Derfor går mobil-linket i LAN-tilstand gennem en Cloudflare-tunnel og ikke direkte til din IP. Tunnel-linket er offentligt og skifter ved hver opstart.

### Konfigurationer

| Konfiguration | IntelliJ | Rider | VS Code |
|---|---|---|---|
| `Infrastructure (docker compose)` | ✓ | ✓ | task `docker compose up` |
| `Ingestion – offline` / `lokal` / `LAN` | ✓ | – | ✓ |
| `Api – offline` / `lokal` / `LAN` (REST + SignalR on port 5262, no UI) | – | ✓ (`processing-server/`) | ✓ |
| `Dashboard – offline` / `lokal` / `LAN` (the dashboard on port 5111, needs the Api running) | – | ✓ (`dashboard/`) | ✓ |
| `Alt – lokal` / `Alt – LAN` (ingestion + Api + dashboard sammen) | – | – | ✓ |

IntelliJ cannot run .NET and Rider cannot run Maven. Run ingestion from IntelliJ and the Api and dashboard from Rider, or everything from VS Code.

The Api and the dashboard are two separate solutions. In Rider, open `processing-server/CrowdControl.Processing.sln` and `dashboard/CrowdControl.Dashboard.sln` in two windows and start `Api – …` in one and `Dashboard – …` in the other, in the same mode. The dashboard calls the Api on the same host name, port 5262 (`ApiBaseUrl` in `dashboard/src/CrowdControl.Dashboard/wwwroot/appsettings.json`). The Api no longer serves the dashboard.

### Links

Hver applikation printer sine links i konsollen, når den er startet:

```
Mobil-app (denne maskine): http://localhost:8080/?event=00000000-0000-0000-0000-000000000001
Mobil-app (offentligt link via Cloudflare-tunnel): https://<tilfældig>.trycloudflare.com/?event=...   (kun LAN)
API (this machine): http://localhost:5262
API (LAN):          http://192.168.x.x:5262                                                         (kun LAN)
Now listening on: http://localhost:5111                                                              (dashboard)
```

Open the dashboard at http://localhost:5111. In LAN mode the dashboard's dev server only prints `0.0.0.0`, so use the LAN IP the Api prints with port 5111 instead (e.g. `http://192.168.x.x:5111`).

RabbitMQ's management-UI ligger på http://localhost:15672 (bruger og kodeord: `crowdcontrol`).

### Første gang

1. **Slet gamle, lokale konfigurationer**, så kun de fælles er tilbage. Det gælder fx `IngestionServerApplication`, `ingestion-server [-pl,bootstrap...]` og `CrowdControl.Api: http/https/local` eller `CrowdControl.Dashboard: http/https`. Gør det under *Edit Configurations…*: de fælles har fluebenet *Store as project file* slået til, så slet dem, der ikke har.
2. **Installér `cloudflared`** (kun til LAN):
   - Windows: `winget install --id Cloudflare.cloudflared`
   - macOS: `brew install cloudflared`
   - Linux: se [Cloudflares downloadside](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/)

   Genstart IDE'en bagefter, så den får den nye `PATH`. Uden `cloudflared` starter ingestion stadig, men printer en advarsel i stedet for et tunnel-link.
3. **VS Code:** installér de anbefalede udvidelser (C# og Docker), når VS Code foreslår dem. Java-udvidelser er ikke nødvendige, fordi ingestion køres via Maven.
4. **Windows-firewall:** første gang du kører `Api – LAN` eller `Dashboard – LAN`, spørger Windows, om programmet må bruge netværket. Tillad det for private netværk, ellers kan andre ikke åbne dashboardet.

### Fejlfinding

- **Port 8080, 5262 eller 5111 er optaget:** en anden instans kører allerede, fx fra en anden IDE eller terminal. Stop den først.
- **Ingen prikker på dashboardet:** ingestion kører i `offline`, så positionerne kun bliver logget. Brug `lokal` eller `LAN`.
- **The dashboard loads but the map never updates:** the Api is not running, or runs in another mode than the dashboard. Start `Api – …` in the same mode.
- **http://localhost:5262 shows 404:** expected. The Api only serves REST and SignalR; the dashboard is on port 5111.
- **Tunnelen lever videre efter stop:** hvis ingestion blev dræbt hårdt, kan `cloudflared` køre videre. Stop processen `cloudflared` manuelt.
