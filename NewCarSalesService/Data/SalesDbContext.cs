using Microsoft.EntityFrameworkCore;
using NewCarSalesService.Models;

public class SalesDbContext : DbContext
{
    public DbSet<NewSales> Transactions { get; set; }

    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("new_sales");
        base.OnModelCreating(modelBuilder);
    }
}