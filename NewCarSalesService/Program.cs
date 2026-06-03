using Microsoft.EntityFrameworkCore;
using NewCarSalesService.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5004);
});

builder.Services.AddControllers();
builder.Services.AddSingleton<EventPublisher>();

var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "dealer";
var dbPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "dealer123";
var connString = $"Host={dbHost};Port=5432;Database=dealer_db;Username={dbUser};Password={dbPass};SearchPath=new_sales";

builder.Services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    context.Database.EnsureCreated();
}

app.MapControllers();
app.Run();