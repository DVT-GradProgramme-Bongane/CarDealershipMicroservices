using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AccessoriesDbContext>(options =>
    options.UseNpgsql(PostgresConnectionString.Build(builder.Configuration)));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

var app = builder.Build();

await AccessoriesSchemaInitializer.EnsureCreatedAsync(app.Services);

app.MapOpenApi();
app.MapScalarApiReference("/scalar", options =>
{
    options.WithTitle("Accessories & Suppliers API");
});

app.MapControllers();

app.Run();
