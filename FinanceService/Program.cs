using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using FinanceService.Services;
using FinanceService.Messaging.Publishers;
using FinanceService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5006);
});

var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "dealer";
var dbPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "dealer123";
var connString = $"Host={dbHost};Port=5432;Database=dealer_db;Username={dbUser};Password={dbPass};SearchPath=new_sales";

builder.Services.AddDbContext<FinancingDbContext>(options =>
    options.UseNpgsql(connString)
        .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
});

builder.Services.AddSingleton<IFinancingEventPublisher, FinancingEventPublisher>();
builder.Services.AddScoped<IFinancingApplicationService, FinancingApplicationService>();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FinancingDbContext>();
    context.Database.EnsureCreated();
}

app.MapControllers();
app.Run();