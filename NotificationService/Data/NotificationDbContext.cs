using Microsoft.EntityFrameworkCore;
using CarDealerShipMicroService.NotificationService.Data.Model;

namespace CarDealerShipMicroService.NotificationService.Data;

public class NotificationDbContext : DbContext
{
    
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications { get; set; }
}