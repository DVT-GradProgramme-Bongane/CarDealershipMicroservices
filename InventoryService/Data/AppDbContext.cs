using Microsoft.EntityFrameworkCore;
using Inventory.Api.Models;

namespace Inventory.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Car> Cars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");

        modelBuilder.Entity<Car>(e => {
            e.ToTable("cars");
            e.HasIndex(c => c.Vin).IsUnique();
            e.HasIndex(c => c.Status);
            e.Property(c => c.Type).HasConversion<string>();
            e.Property(c => c.Status).HasConversion<string>();
        });
    }
}