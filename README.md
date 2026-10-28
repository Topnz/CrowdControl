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
| `Api – offline` / `lokal` / `LAN` (serverer også dashboardet) | – | ✓ | ✓ |
| `Alt – lokal` / `Alt – LAN` (ingestion + Api sammen) | – | – | ✓ |

IntelliJ kan ikke køre .NET, og Rider kan ikke køre Maven. Kør derfor ingestion fra IntelliJ og Api'et fra Rider, eller det hele fra VS Code.

### Links

Hver applikation printer sine links i konsollen, når den er startet:

```
Mobil-app (denne maskine): http://localhost:8080/?event=00000000-0000-0000-0000-000000000001
Mobil-app (offentligt link via Cloudflare-tunnel): https://<tilfældig>.trycloudflare.com/?event=...   (kun LAN)
Dashboard (denne maskine): http://localhost:5262
Dashboard (LAN):           http://192.168.x.x:5262                                                   (kun LAN)
```

RabbitMQ's management-UI ligger på http://localhost:15672 (bruger og kodeord: `crowdcontrol`).

### Første gang

1. **Slet gamle, lokale konfigurationer**, så kun de fælles er tilbage. Det gælder fx `IngestionServerApplication`, `ingestion-server [-pl,bootstrap...]` og `CrowdControl.Api: http/https/local` eller `CrowdControl.Dashboard: http/https`. Gør det under *Edit Configurations…*: de fælles har fluebenet *Store as project file* slået til, så slet dem, der ikke har.
2. **Installér `cloudflared`** (kun til LAN):
   - Windows: `winget install --id Cloudflare.cloudflared`
   - macOS: `brew install cloudflared`
   - Linux: se [Cloudflares downloadside](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/)

   Genstart IDE'en bagefter, så den får den nye `PATH`. Uden `cloudflared` starter ingestion stadig, men printer en advarsel i stedet for et tunnel-link.
3. **VS Code:** installér de anbefalede udvidelser (C# og Docker), når VS Code foreslår dem. Java-udvidelser er ikke nødvendige, fordi ingestion køres via Maven.
4. **Windows-firewall:** første gang du kører `Api – LAN`, spørger Windows, om programmet må bruge netværket. Tillad det for private netværk, ellers kan andre ikke åbne dashboardet.

### Fejlfinding

- **Port 8080 eller 5262 er optaget:** en anden instans kører allerede, fx fra en anden IDE eller terminal. Stop den først.
- **Ingen prikker på dashboardet:** ingestion kører i `offline`, så positionerne kun bliver logget. Brug `lokal` eller `LAN`.
- **Tunnelen lever videre efter stop:** hvis ingestion blev dræbt hårdt, kan `cloudflared` køre videre. Stop processen `cloudflared` manuelt.
