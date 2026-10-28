using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CrowdControl.Dashboard.Services;
using CrowdControl.Dashboard;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ApiAddress api = ApiAddress.From(builder.Configuration["ApiBaseUrl"], new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddSingleton(api);
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = api.BaseUrl });
builder.Services.AddScoped<MapService>();

await builder.Build().RunAsync();
