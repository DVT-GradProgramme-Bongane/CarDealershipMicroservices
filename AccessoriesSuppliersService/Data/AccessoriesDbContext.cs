using AccessoriesSuppliersService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessoriesSuppliersService.Data;

public sealed class AccessoriesDbContext(DbContextOptions<AccessoriesDbContext> options) : DbContext(options)
{
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<AccessoryItem> Items => Set<AccessoryItem>();
    public DbSet<AccessoryOrder> Orders => Set<AccessoryOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("accessories");

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(supplier => supplier.Id);
            entity.Property(supplier => supplier.Id).HasColumnName("id");
            entity.Property(supplier => supplier.Name).HasColumnName("name").IsRequired();
            entity.Property(supplier => supplier.Contact).HasColumnName("contact").IsRequired();
            entity.Property(supplier => supplier.Email).HasColumnName("email").IsRequired();
        });

        modelBuilder.Entity<AccessoryItem>(entity =>
        {
            entity.ToTable("items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("id");
            entity.Property(item => item.SupplierId).HasColumnName("supplier_id").IsRequired();
            entity.Property(item => item.Name).HasColumnName("name").IsRequired();
            entity.Property(item => item.Price).HasColumnName("price").HasColumnType("decimal").IsRequired();
            entity.Property(item => item.Stock).HasColumnName("stock").IsRequired();
            entity.HasOne(item => item.Supplier)
                .WithMany(supplier => supplier.Items)
                .HasForeignKey(item => item.SupplierId);
        });

        modelBuilder.Entity<AccessoryOrder>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Id).HasColumnName("id");
            entity.Property(order => order.ItemId).HasColumnName("item_id").IsRequired();
            entity.Property(order => order.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(order => order.Status).HasColumnName("status").IsRequired();
            entity.Property(order => order.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").IsRequired();
            entity.HasOne(order => order.Item)
                .WithMany(item => item.Orders)
                .HasForeignKey(order => order.ItemId);
        });
    }
}
