using Microsoft.EntityFrameworkCore;

namespace CrowdControl.Infrastructure.Persistence;

public class MainDbContext(DbContextOptions<MainDbContext> options) : DbContext(options)
{
    // Tilføj DbSet<T> for hver entitet, fx:
    // public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Finder alle IEntityTypeConfiguration<T> i Configurations/
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
    }
}
