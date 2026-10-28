using CrowdControl.Api;
using CrowdControl.Api.Hubs;
using CrowdControl.Application;
using CrowdControl.Infrastructure.Messaging;
using CrowdControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Static web assets (Dashboard) are only loaded automatically in Development
if (builder.Environment.IsEnvironment("Offline"))
    builder.WebHost.UseStaticWebAssets();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();

// The dashboard can run on its own origin (the launch profiles in CrowdControl.Dashboard, port 5111).
// SignalR requires AllowCredentials and therefore explicit origins. In Development/Offline all
// origins are allowed, since LAN mode opens the dashboard via changing LAN IPs.
string[] dashboardOrigins = builder.Configuration.GetSection("Dashboard:Origins").Get<string[]>() ?? [];
bool allowAnyOrigin = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Offline");
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .SetIsOriginAllowed(origin => allowAnyOrigin || dashboardOrigins.Contains(origin))
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// Disabled: the demo event is no longer used; the map only shows positions from the queue
// // Demo event as in ingestion-server's DemoEventSeeder: rectangle around Denmark,
// // split into two zones at the Great Belt
// const double denmarkSouth = 54.5;
// const double denmarkNorth = 57.8;
// const double denmarkWest = 8.0;
// const double denmarkEast = 15.2;
// const double zoneSplitLongitude = 11.0;
//
// Event demoEvent = new Event
// {
//     Name = "Demo-event",
//     // Starts an hour before now, so phones with a slightly wrong clock are not rejected
//     StartTime = DateTime.UtcNow.AddHours(-1),
//     EndTime = DateTime.UtcNow.AddDays(30),
//     SendInterval = 10,
//     Perimiter = Rectangle(denmarkSouth, denmarkNorth, denmarkWest, denmarkEast),
//     Zones = new List<Zone>
//     {
//         new Zone { Name = "Vestdanmark", Boundary = Rectangle(denmarkSouth, denmarkNorth, denmarkWest, zoneSplitLongitude) },
//         new Zone { Name = "Østdanmark", Boundary = Rectangle(denmarkSouth, denmarkNorth, zoneSplitLongitude, denmarkEast) }
//     }
// };
//
// Boundary Rectangle(double south, double north, double west, double east)
// {
//     Boundary boundary = new Boundary();
//     boundary.AddGeoPoints(new List<GeoPoint>
//     {
//         new GeoPoint { Latitude = south, Longitude = west },
//         new GeoPoint { Latitude = south, Longitude = east },
//         new GeoPoint { Latitude = north, Longitude = east },
//         new GeoPoint { Latitude = north, Longitude = west }
//     });
//     return boundary;
// }
//
// builder.Services.AddSingleton(new GridAggregator(demoEvent));
builder.Services.AddSingleton(new GridAggregator("Positioner fra køen"));
builder.Services.AddSingleton<LatestHeatmap>();
builder.Services.AddHostedService<HeatmapBroadcaster>();
builder.Services.AddSingleton<IReceivePositionUseCase, ReceivePositionService>();

// Offline runs without RabbitMQ (fake messaging comes later)
if (!builder.Environment.IsEnvironment("Offline"))
    builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

// Updates main-db to the latest EF Core migration at startup (requires docker compose)
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Offline"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<MainDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

// Serves the Blazor WASM dashboard (CrowdControl.Dashboard)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors();

app.MapControllers();
app.MapHub<HeatmapHub>("/hubs/heatmap");
app.MapFallbackToFile("index.html");

StartupLinks.PrintOnStarted(app);

app.Run();
