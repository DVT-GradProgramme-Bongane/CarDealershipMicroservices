using Microsoft.Extensions.Configuration;

namespace ClientService;

public static class EnvConfiguration
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (string.IsNullOrEmpty(host))
        {
            return "Host=localhost;Port=5432;Database=dealer;Username=dealer;Password=dealer123";
        }
        
        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}