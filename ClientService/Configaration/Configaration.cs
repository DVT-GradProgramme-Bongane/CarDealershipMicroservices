namespace ClientService;

public static class EnvConfiguration
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var host     = Environment.GetEnvironmentVariable("POSTGRES_HOST")     ?? configuration["POSTGRES_HOST"];
        var port     = Environment.GetEnvironmentVariable("POSTGRES_PORT")     ?? configuration["POSTGRES_PORT"];
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB")       ?? configuration["POSTGRES_DB"];
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER")     ?? configuration["POSTGRES_USER"];
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? configuration["POSTGRES_PASSWORD"];

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}