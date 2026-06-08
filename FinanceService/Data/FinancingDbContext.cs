using Microsoft.EntityFrameworkCore;

public class FinancingDbContext : DbContext
{
    public FinancingDbContext(DbContextOptions<FinancingDbContext> options): base(options) {}
	public DbSet<FinancingApplication> Applications => Set<FinancingApplication>();

	protected override void OnModelCreating(ModelBuilder modelBuilder){
	modelBuilder.HasDefaultSchema("financing");
	modelBuilder.Entity<FinancingApplication>(entity => {
	entity.ToTable("applications");
	entity.HasKey(e => e.Id);
	//I read that these hasprecision etc methods are good for decimal columns otherwise efcore chooses crazy defaults
	 entity.Property(e => e.LoanAmount).HasPrecision(18, 2);
     entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
     entity.Property(e => e.Status).HasMaxLength(20);
});
	}
}