using Microsoft.EntityFrameworkCore;

namespace CrowdControl.Infrastructure.Persistence;

public class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Finder alle IEntityTypeConfiguration<T> i Configurations/
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
    }
}
