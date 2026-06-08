namespace MaintenanceService.Data;
using MaintenanceService.Models;
using Microsoft.EntityFrameworkCore;


public class MaintenanceDbContext : DbContext
{
    public MaintenanceDbContext(DbContextOptions<MaintenanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<MaintenanceJob> Jobs => Set<MaintenanceJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("maintenance");

        modelBuilder.Entity<MaintenanceJob>(entity =>
        {
            entity.ToTable("jobs");
            entity.HasIndex(job => job.CarId);
            entity.HasIndex(job => job.ClientId);
            entity.HasIndex(job => job.StaffId);
            entity.HasIndex(job => job.Status);
        });
    }
}
