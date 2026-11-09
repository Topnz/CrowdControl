using CrowdControl.Application.ManageEvent;
using CrowdControl.Infrastructure.Persistence.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdControl.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers MainDbContext (used for migrations) and the repositories chosen by the setting
    /// "Persistence:Mode": "InMemory" (default) or "Database".
    /// </summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MainDb")
            ?? throw new InvalidOperationException("Connection string 'MainDb' mangler.");

        services.AddDbContext<MainDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        string mode = configuration["Persistence:Mode"] ?? "InMemory";
        switch (mode)
        {
            case "InMemory":
                AddInMemoryRepositories(services);
                break;
            case "Database":
                // EF Core repositories are registered here once they exist, e.g.:
                // services.AddScoped<IEventRepository, EfEventRepository>();
                throw new InvalidOperationException(
                    "Persistence:Mode 'Database' has no EF Core repositories yet. Use 'InMemory'.");
            default:
                throw new InvalidOperationException(
                    $"Unknown Persistence:Mode '{mode}'. Use 'InMemory' or 'Database'.");
        }

        return services;
    }

    // Kept in memory for the lifetime of the Api, so data is lost on restart
    private static void AddInMemoryRepositories(IServiceCollection services)
    {
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
    }
}
