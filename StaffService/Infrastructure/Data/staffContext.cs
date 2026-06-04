using Microsoft.EntityFrameworkCore;

public class StaffDBContext :DbContext
{
    public StaffDBContext(DbContextOptions<StaffDBContext> options) : base(options)
    {
    }

    public DbSet<StaffEntitiy> Staff => Set<StaffEntitiy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}