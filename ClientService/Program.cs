using ClientService;
using ClientService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5003);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

Console.WriteLine(builder.Configuration["POSTGRES_HOST"]);

var connectionString = EnvConfiguration.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ClientDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ClientDbContext>();
        
        dbContext.Database.EnsureCreated();
        
        Console.WriteLine("PostgreSQL 'clients' schema and 'customers' table are verified and ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed on early boot: {ex.Message}");
        Console.WriteLine("Retrying schema validation in 5 seconds...");
        Thread.Sleep(5000);
        
        var dbContext = services.GetRequiredService<ClientDbContext>();
        dbContext.Database.EnsureCreated();
    }
}

app.UseAuthorization();
app.MapControllers();

app.Run();