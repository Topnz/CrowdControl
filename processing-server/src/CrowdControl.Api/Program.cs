using CrowdControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Static web assets (Dashboard) indlæses kun automatisk i Development
if (builder.Environment.IsEnvironment("Local"))
    builder.WebHost.UseStaticWebAssets();

builder.Services.AddPersistence(builder.Configuration);

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

app.MapFallbackToFile("index.html");

app.Run();
