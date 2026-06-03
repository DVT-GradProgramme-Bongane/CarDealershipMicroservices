using Microsoft.EntityFrameworkCore;

public class StaffContext :DbContext
{
    public StaffContext(DbContextOptions<StaffContext> options) : base(options)
    {
    }

    public DbSet<Staff> Staff => Set<Staff>();

}