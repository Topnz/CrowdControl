using Microsoft.EntityFrameworkCore;

namespace CrowdControl.Infrastructure.Persistence;

public class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options)
{
    // Add a DbSet<T> for each entity, e.g.:
    // public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Finds all IEntityTypeConfiguration<T> in Configurations/
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
    }
}
