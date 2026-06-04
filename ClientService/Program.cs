using ClientServices;
using ClientServices.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = EnvConfiguration.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ClientDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var retries = 5;

    for (int i = 0; i < retries; i++)
    {
        try
        {
            var dbContext = services.GetRequiredService<ClientDbContext>();
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Database is ready.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB not ready (attempt {i + 1}/{retries}): {ex.Message}");
            if (i == retries - 1) throw;
            Thread.Sleep(5000);
        }
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();