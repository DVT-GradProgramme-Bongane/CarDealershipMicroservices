using Microsoft.EntityFrameworkCore;
using UsedCarSalesService.Models;

namespace UsedCarSalesService.Data;

public class UsedCarSalesDbContext : DbContext
{
    public UsedCarSalesDbContext(DbContextOptions<UsedCarSalesDbContext> options) : base(options) { }

    public DbSet<UsedSalesTransaction> Transactions => Set<UsedSalesTransaction>();
}