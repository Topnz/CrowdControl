var builder = WebApplication.CreateBuilder(args);

// Static web assets (Dashboard) indlæses kun automatisk i Development
if (builder.Environment.IsEnvironment("Local"))
    builder.WebHost.UseStaticWebAssets();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

// Serverer Blazor WASM-dashboardet (CrowdControl.Dashboard)
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();