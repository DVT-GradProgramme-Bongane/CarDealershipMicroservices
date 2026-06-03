namespace UsedCarSalesService.Data;

public class UsedCarSalesDbContext : DbContext
{
    public UsedCarSalesDbContext(DbContextOptions options) : base(options) { }

    public DbSet<UsedSalesTransaction> Transactions => Set<UsedSalesTransaction>();
}