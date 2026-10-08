using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdControl.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MainDbContext>(options => options
            .UseNpgsql(configuration.GetConnectionString("MainDb"))
            .UseSnakeCaseNamingConvention());

        // Repositories registreres her, fx:
        // services.AddScoped<IEventRepository, EfEventRepository>();

        return services;
    }
}
