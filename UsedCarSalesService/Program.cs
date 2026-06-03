using Microsoft.EntityFrameworkCore;
using UsedCarSalesService.Data;
using UsedCarSalesService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString =
    builder.Configuration.GetConnectionString("UsedCarSalesDb")
    ?? "Host=localhost;Port=5432;Database=used_car_sales;Username=postgres;Password=postgres";

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
