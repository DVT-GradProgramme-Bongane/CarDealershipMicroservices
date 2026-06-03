using Inventory.Api.Data;
using Inventory.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// RabbitMqPublisher uses async init so register via factory
builder.Services.AddSingleton(sp =>
    RabbitMqPublisher.CreateAsync(sp.GetRequiredService<IConfiguration>()).GetAwaiter().GetResult());

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();
app.Run();