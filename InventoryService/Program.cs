using Inventory.Api.Data;
using Inventory.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    $"Host={builder.Configuration["POSTGRES_HOST"]};" +
    $"Port={builder.Configuration["POSTGRES_PORT"]};" +
    $"Database={builder.Configuration["POSTGRES_DB"]};" +
    $"Username={builder.Configuration["POSTGRES_USER"]};" +
    $"Password={builder.Configuration["POSTGRES_PASSWORD"]};" +
    "SearchPath=inventory";

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(connectionString));

// RabbitMqPublisher uses async init so register via factory
builder.Services.AddSingleton(sp =>
    RabbitMqPublisher.CreateAsync(sp.GetRequiredService<IConfiguration>()).GetAwaiter().GetResult());

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.CamelCase, allowIntegerValues: false)));
builder.Services.AddGrpc();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.Migrate();
}

app.MapControllers();
app.MapGrpcService<InventoryGrpcService>();
app.Run();