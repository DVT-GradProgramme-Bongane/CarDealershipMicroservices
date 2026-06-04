using Microsoft.EntityFrameworkCore;
using CarDealerShipMicroService.NotificationService.Data;
using CarDealerShipMicroService.NotificationService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<RabbitMqConsumer>();

var host = builder.Configuration["POSTGRES_HOST"]
           ?? throw new InvalidOperationException("POSTGRES_HOST is not configured.");

var port = builder.Configuration["POSTGRES_PORT"]
           ?? throw new InvalidOperationException("POSTGRES_PORT is not configured.");

var database = builder.Configuration["POSTGRES_DB"]
               ?? throw new InvalidOperationException("POSTGRES_DB is not configured.");

var user = builder.Configuration["POSTGRES_USER"]
           ?? throw new InvalidOperationException("POSTGRES_USER is not configured.");

var password = builder.Configuration["POSTGRES_PASSWORD"]
               ?? throw new InvalidOperationException("POSTGRES_PASSWORD is not configured.");

var connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db_context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    db_context.Database.EnsureCreated();
}

app.Run();