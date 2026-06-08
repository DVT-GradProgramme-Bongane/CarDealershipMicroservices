using MaintenanceService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5008);
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? builder.Configuration["POSTGRES_HOST"]};" +
    $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? builder.Configuration["POSTGRES_PORT"]};" +
    $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB") ?? builder.Configuration["POSTGRES_DB"]};" +
    $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER") ?? builder.Configuration["POSTGRES_USER"]};" +
    $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? builder.Configuration["POSTGRES_PASSWORD"]}";
builder.Services.AddDbContext<MaintenanceDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var retries = 5;

    for (var i = 0; i < retries; i++)
    {
        try
        {
            var dbContext = services.GetRequiredService<MaintenanceDbContext>();
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Maintenance database is ready.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Maintenance DB not ready (attempt {i + 1}/{retries}): {ex.Message}");
            if (i == retries - 1)
            {
                throw;
            }

            Thread.Sleep(5000);
        }
    }
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
