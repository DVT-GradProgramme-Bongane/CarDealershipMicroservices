using CarDealerShipMicroService.NotificationService.Data;
using Microsoft.EntityFrameworkCore;

namespace CarDealerShipMicroService.NotificationService.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/notifications", async (NotificationDbContext database) =>
        {
            var notifications = await database.Notifications
                .OrderByDescending(notification => notification.CreatedAt)
                .ToListAsync();
            return Results.Ok(notifications);
        });

        app.MapGet("/notifications/{id}", async (Guid id, NotificationDbContext database) =>
        {
            var notification = await database.Notifications.FindAsync(id);
            return notification is null ? Results.NotFound() : Results.Ok(notification);
        });

        return app;
    }
}
