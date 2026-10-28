# CrowdControl

A live heatmap of where participants are at an event. Architecture and decisions are described in [`CLAUDE.md`](CLAUDE.md).

## Running the project

Everyone has the same run configurations in IntelliJ, Rider and VS Code. Pick them in the dropdown next to the green run button.

### Modes

| Mode | What it does | Needs |
|---|---|---|
| **offline** | Runs without RabbitMQ and PostGIS. Ingestion logs the positions instead of passing them on. The dashboard shows fake data. | The Api still uses main-db, so start `Infrastructure (docker compose)` if you run the Api |
| **lokal** | The whole system on your own machine, reachable from `localhost` only. | Docker (started automatically) |
| **LAN** | Like `lokal`, but others can open links: the mobile app through a Cloudflare tunnel (HTTPS) and the dashboard through your LAN IP. | Docker (started automatically) and `cloudflared` |

The mobile app uses the Geolocation API, which browsers only allow on `https://` or `localhost`. That is why the mobile link in LAN mode goes through a Cloudflare tunnel and not straight to your IP. The tunnel link is public and changes at every start.

### Configurations

| Configuration | IntelliJ | Rider | VS Code |
|---|---|---|---|
| `Infrastructure (docker compose)` | ✓ | ✓ | task `docker compose up` |
| `Ingestion – offline` / `lokal` / `LAN` | ✓ | – | ✓ |
| `Api – offline` / `lokal` / `LAN` (REST + SignalR on port 5262, no UI) | – | ✓ (`processing-server/`) | ✓ |
| `Dashboard – offline` / `lokal` / `LAN` (the dashboard on port 5111; `offline` uses fake data, `lokal`/`LAN` need the Api running) | – | ✓ (`dashboard/`) | ✓ |
| `Alt – lokal` / `Alt – LAN` (ingestion + Api + dashboard together) | – | – | ✓ |

IntelliJ cannot run .NET and Rider cannot run Maven. Run ingestion from IntelliJ and the Api and dashboard from Rider, or everything from VS Code.

The Api and the dashboard are two separate solutions. In Rider, open `processing-server/CrowdControl.Processing.sln` and `dashboard/CrowdControl.Dashboard.sln` in two windows and start `Api – …` in one and `Dashboard – …` in the other, in the same mode. The dashboard calls the Api on the same host name, port 5262 (`ApiBaseUrl` in `dashboard/src/CrowdControl.Dashboard/wwwroot/appsettings.json`). The Api does not serve the dashboard.

### Links

Every application prints its links in the console once it has started:

```
Mobile app (this machine): http://localhost:8080/?event=00000000-0000-0000-0000-000000000001
Mobile app (public link via Cloudflare tunnel): https://<random>.trycloudflare.com/?event=...      (LAN only)
API (this machine):        http://localhost:5262
API (LAN):                 http://192.168.x.x:5262                                                  (LAN only)
Dashboard (this machine):  http://localhost:5111
Dashboard (LAN):           http://192.168.x.x:5111                                                  (LAN only)
Dashboard with fake data:  http://localhost:5111/?feed=fake   (no Api needed)
```

The dashboard's dev server cannot print links of its own (it only prints `Now listening on`), so the Api prints the dashboard's links too.

**Fake data:** opening the dashboard with `?feed=fake` (http://localhost:5111/?feed=fake) makes it use random data instead of the Api. `Dashboard – offline` opens that address by itself, so it runs without the Api and docker. Without `?feed=fake` the dashboard always uses the real Api. (The launch profile's environment cannot choose this: .NET 10 fixes a standalone WebAssembly app's environment when it is built.)

The RabbitMQ management UI is at http://localhost:15672 (user and password: `crowdcontrol`).

### First time

1. **Install the tools:**
   - The **.NET 10 SDK** (`winget install Microsoft.DotNet.SDK.10`). With only SDK 9, every C# project fails with `NETSDK1045`.
   - **Docker Desktop** (`winget install Docker.DockerDesktop`) for `lokal` and `LAN`. Start it once and accept the terms before the first run.
   - **`cloudflared`** (LAN only):
     - Windows: `winget install --id Cloudflare.cloudflared`
     - macOS: `brew install cloudflared`
     - Linux: see [Cloudflare's download page](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/)

     Without `cloudflared`, ingestion still starts but prints a warning instead of a tunnel link.

   Restart your IDE and terminals afterwards, so they get the new `PATH`.
2. **Delete old, local configurations**, so only the shared ones are left. For example `IngestionServerApplication`, `ingestion-server [-pl,bootstrap...]`, `CrowdControl.Api: http/https/local` or `CrowdControl.Dashboard: http/https`. Do it under *Edit Configurations…*: the shared ones have *Store as project file* ticked, so delete those that do not.
3. **VS Code:** install the recommended extensions (C# and Docker) when VS Code suggests them. Java extensions are not needed, because ingestion runs through Maven.
4. **Windows firewall:** the first time you run `Api – LAN` or `Dashboard – LAN`, Windows asks whether the program may use the network. Allow it for private networks, otherwise others cannot open the dashboard.

### Troubleshooting

- **Port 8080, 5262 or 5111 is in use:** another instance is already running, e.g. from another IDE or terminal. Stop it first.
- **The Api fails at startup with `password authentication failed for user "crowdcontrol"`:** the Api reaches another PostgreSQL instead of docker's main-db. main-db listens on host port 5434 (not 5432, which a PostgreSQL installed directly on Windows often uses). Check that main-db was recreated after pulling (`docker compose up -d` does it) and that nothing else uses port 5434.
- **No dots on the dashboard:** ingestion runs in `offline`, so positions are only logged. Use `lokal` or `LAN`, or open the dashboard with `?feed=fake`.
- **The dashboard loads but the map never updates, or shows "An unhandled error has occurred":** the Api is not running, or runs in another mode than the dashboard. Start `Api – …` in the same mode, or open the dashboard with `?feed=fake` to use fake data.
- **The dots are one small blob:** the map starts zoomed out over Zealand. Zoom in on the event's area.
- **http://localhost:5262 shows 404:** expected. The Api only serves REST and SignalR; the dashboard is on port 5111.
- **The tunnel lives on after stopping:** if ingestion was killed hard, `cloudflared` can keep running. Stop the `cloudflared` process manually.
