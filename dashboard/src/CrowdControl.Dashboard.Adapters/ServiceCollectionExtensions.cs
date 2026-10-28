using CrowdControl.Dashboard.Core.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdControl.Dashboard.Adapters;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the heatmap feed. <paramref name="useFake"/> is asked when the feed is first
    /// resolved: true gives random data without the Api, false the real feed via SignalR.
    /// </summary>
    public static IServiceCollection AddHeatmapFeed(this IServiceCollection services, ApiAddress api, Func<IServiceProvider, bool> useFake)
    {
        services.AddSingleton(api);
        services.AddScoped<IHeatmapFeed>(serviceProvider => useFake(serviceProvider) ? new FakeHeatmapFeed() : new SignalRHeatmapFeed(api));
        return services;
    }
}
