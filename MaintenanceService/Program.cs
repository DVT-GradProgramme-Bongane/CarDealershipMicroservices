using MaintenanceService.Data;
using MaintenanceService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5008);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<InventoryGrpcClient>();

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? builder.Configuration["POSTGRES_HOST"]};" +
    $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? builder.Configuration["POSTGRES_PORT"]};" +
    $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB") ?? builder.Configuration["POSTGRES_DB"]};" +
    $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER") ?? builder.Configuration["POSTGRES_USER"]};" +
    $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? builder.Configuration["POSTGRES_PASSWORD"]}";
builder.Services.AddDbContext<MaintenanceDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
    context.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
