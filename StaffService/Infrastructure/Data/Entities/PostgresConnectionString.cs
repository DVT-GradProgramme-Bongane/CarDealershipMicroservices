using Microsoft.Extensions.Configuration;

public static class PostgresConnectionString
{
    public static string Build(IConfiguration configuration)
    {
        var configured = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        var host = configuration["POSTGRES_HOST"] ;
        var port = configuration["POSTGRES_PORT"] ;
        var user = configuration["POSTGRES_USER"] ;
        var password = configuration["POSTGRES_PASSWORD"] ;
        var database = configuration["POSTGRES_DB"] ;

        return $"Host={host};Port={port};Username={user};Password={password};Database={database}";
    }
}