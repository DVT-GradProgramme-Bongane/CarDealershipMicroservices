using Microsoft.EntityFrameworkCore;
using Npgsql;
using UsedCarSalesService.Data;
using UsedCarSalesService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER")
    ?? throw new InvalidOperationException("POSTGRES_USER is required.");
var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
    ?? throw new InvalidOperationException("POSTGRES_PASSWORD is required.");
var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var dbName = Environment.GetEnvironmentVariable("POSTGRES_DB");

var baseConnectionString =
    builder.Configuration.GetConnectionString("UsedCarSalesDb")
    ?? "Host=localhost;Port=5432;Database=used_car_sales";

var connectionBuilder = new NpgsqlConnectionStringBuilder(baseConnectionString)
{
    Username = dbUser,
    Password = dbPassword
};

if (!string.IsNullOrWhiteSpace(dbHost))
{
    connectionBuilder.Host = dbHost;
}

if (int.TryParse(dbPort, out var parsedPort))
{
    connectionBuilder.Port = parsedPort;
}

if (!string.IsNullOrWhiteSpace(dbName))
{
    connectionBuilder.Database = dbName;
}

var connectionString = connectionBuilder.ConnectionString;

builder.Services.AddDbContext<UsedCarSalesDbContext>(options =>
    options.UseNpgsql(connectionString));

var inventoryGrpcUrl = builder.Configuration["Grpc:InventoryUrl"] ?? "http://localhost:5001";
builder.Services.AddGrpcClient<global::InventoryService.InventoryServiceClient>(options =>
{
    options.Address = new Uri(inventoryGrpcUrl);
});

builder.Services.AddScoped<InventoryGrpcClient>();
builder.Services.AddScoped<UsedSalesService>();
builder.Services.AddSingleton<EventBus>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
