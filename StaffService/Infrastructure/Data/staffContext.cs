using Microsoft.EntityFrameworkCore;

public class StaffDBContext : DbContext
{
    public StaffDBContext(DbContextOptions<StaffDBContext> options) : base(options)
    {
    }

    public DbSet<StaffEntitiy> Staff => Set<StaffEntitiy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StaffEntitiy>(entity =>
        {
            entity.ToTable("employees", "staff");  

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.StaffRole)
                .HasColumnName("role")
                .HasConversion(
                    v => v == Role.finance_manager ? "finance_manager" : v.ToString().ToLower(),
                    v => v == "finance_manager" ? Role.finance_manager : Enum.Parse<Role>(v, ignoreCase: true)
                );
        });
    }
}