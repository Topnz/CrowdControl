using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CrowdControl.Dashboard.Adapters;
using CrowdControl.Dashboard.Services;
using CrowdControl.Dashboard;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ApiAddress api = ApiAddress.From(builder.Configuration["ApiBaseUrl"], new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = api.BaseUrl });

// Opening the dashboard with "?feed=fake" (e.g. http://localhost:5111/?feed=fake) runs it on fake
// data without the Api; the "offline" launch profile opens that address. The environment cannot
// decide this: .NET 10 fixes a standalone WebAssembly app's environment when it is built, so the
// launch profile's ASPNETCORE_ENVIRONMENT never reaches the browser.
builder.Services.AddHeatmapFeed(api, sp => FakeFeedRequested(sp.GetRequiredService<NavigationManager>().Uri));

builder.Services.AddScoped<MapService>();

await builder.Build().RunAsync();

static bool FakeFeedRequested(string address) =>
    new Uri(address).Query.TrimStart('?').Split('&').Contains("feed=fake", StringComparer.OrdinalIgnoreCase);
