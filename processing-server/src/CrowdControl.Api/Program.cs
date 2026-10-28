using CrowdControl.Api;
using CrowdControl.Api.Hubs;
using CrowdControl.Application;
using CrowdControl.Application.MonitorEvent;
using CrowdControl.Infrastructure.Messaging;
using CrowdControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Static web assets (Dashboard) indlæses kun automatisk i Development
if (builder.Environment.IsEnvironment("Offline"))
    builder.WebHost.UseStaticWebAssets();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Dashboardet kan køre på sin egen origin (launch-profilerne i CrowdControl.Dashboard, port 5111).
// SignalR kræver AllowCredentials og dermed eksplicitte origins. I Development/Offline tillades
// alle origins, da LAN-tilstanden åbner dashboardet via skiftende LAN-IP'er.
string[] dashboardOrigins = builder.Configuration.GetSection("Dashboard:Origins").Get<string[]>() ?? [];
bool allowAnyOrigin = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Offline");
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .SetIsOriginAllowed(origin => allowAnyOrigin || dashboardOrigins.Contains(origin))
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

// Slået fra: demo-eventet bruges ikke længere; kortet viser kun positioner fra køen
// // Demo-event som i ingestion-serverens DemoEventSeeder: firkant omkring Danmark,
// // delt i to zoner ved Storebælt
// const double denmarkSouth = 54.5;
// const double denmarkNorth = 57.8;
// const double denmarkWest = 8.0;
// const double denmarkEast = 15.2;
// const double zoneSplitLongitude = 11.0;
//
// Event demoEvent = new Event
// {
//     Name = "Demo-event",
//     // Starter en time før nu, så telefoner med lidt forkert ur ikke afvises
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

// Offline kører uden RabbitMQ (fake-messaging kommer senere)
if (!builder.Environment.IsEnvironment("Offline"))
    builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

// Opdaterer main-db til nyeste EF Core-migration ved opstart (kræver docker compose)
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Offline"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<MainDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

// Serverer Blazor WASM-dashboardet (CrowdControl.Dashboard)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors();

app.MapControllers();
app.MapHub<HeatmapHub>("/hubs/heatmap");
app.MapFallbackToFile("index.html");

StartupLinks.PrintOnStarted(app);

app.Run();
