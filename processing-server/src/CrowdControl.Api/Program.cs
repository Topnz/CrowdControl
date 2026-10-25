using CrowdControl.Api.Hubs;
using CrowdControl.Application;
using CrowdControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Static web assets (Dashboard) indlæses kun automatisk i Development
if (builder.Environment.IsEnvironment("Local"))
    builder.WebHost.UseStaticWebAssets();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Demo-event som i ingestion-serverens DemoEventSeeder: firkant omkring Danmark,
// delt i to zoner ved Storebælt
const double denmarkSouth = 54.5;
const double denmarkNorth = 57.8;
const double denmarkWest = 8.0;
const double denmarkEast = 15.2;
const double zoneSplitLongitude = 11.0;

Event demoEvent = new Event
{
    Name = "Demo-event",
    // Starter en time før nu, så telefoner med lidt forkert ur ikke afvises
    StartTime = DateTime.UtcNow.AddHours(-1),
    EndTime = DateTime.UtcNow.AddDays(30),
    SendInterval = 10,
    Perimiter = Rectangle(denmarkSouth, denmarkNorth, denmarkWest, denmarkEast),
    Zones = new List<Zone>
    {
        new Zone { Name = "Vestdanmark", Boundary = Rectangle(denmarkSouth, denmarkNorth, denmarkWest, zoneSplitLongitude) },
        new Zone { Name = "Østdanmark", Boundary = Rectangle(denmarkSouth, denmarkNorth, zoneSplitLongitude, denmarkEast) }
    }
};

Boundary Rectangle(double south, double north, double west, double east)
{
    Boundary boundary = new Boundary();
    boundary.AddGeoPoints(new List<GeoPoint>
    {
        new GeoPoint { Latitude = south, Longitude = west },
        new GeoPoint { Latitude = south, Longitude = east },
        new GeoPoint { Latitude = north, Longitude = east },
        new GeoPoint { Latitude = north, Longitude = west }
    });
    return boundary;
}

builder.Services.AddSingleton(new GridAggregator(demoEvent));
builder.Services.AddSingleton<LatestHeatmap>();
builder.Services.AddHostedService<HeatmapBroadcaster>();

var app = builder.Build();

// Opdaterer main-db til nyeste EF Core-migration ved opstart (kræver docker compose)
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<MainDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

// Serverer Blazor WASM-dashboardet (CrowdControl.Dashboard)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<HeatmapHub>("/hubs/heatmap");
app.MapFallbackToFile("index.html");

app.Run();
