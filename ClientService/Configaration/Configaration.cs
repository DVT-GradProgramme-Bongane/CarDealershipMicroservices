using Microsoft.Extensions.Configuration;

namespace ClientService;

public static class EnvConfiguration
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var host = configuration["POSTGRES_HOST"];
        var port = configuration["POSTGRES_PORT"];
        var database = configuration["POSTGRES_DB"];
        var username = configuration["POSTGRES_USER"];
        var password = configuration["POSTGRES_PASSWORD"];

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}