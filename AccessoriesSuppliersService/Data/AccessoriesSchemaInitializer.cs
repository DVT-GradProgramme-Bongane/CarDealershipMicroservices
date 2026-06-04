using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Threading.Tasks;

namespace AccessoriesSuppliersService.Data;

public static class AccessoriesSchemaInitializer
{
    public static async Task EnsureCreatedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AccessoriesDbContext>();

        await db.Database.ExecuteSqlRawAsync("""
            CREATE SCHEMA IF NOT EXISTS accessories;

            CREATE TABLE IF NOT EXISTS accessories.suppliers (
                id UUID PRIMARY KEY,
                name VARCHAR NOT NULL,
                contact VARCHAR NOT NULL,
                email VARCHAR NOT NULL
            );

            CREATE TABLE IF NOT EXISTS accessories.items (
                id UUID PRIMARY KEY,
                supplier_id UUID NOT NULL REFERENCES accessories.suppliers(id),
                name VARCHAR NOT NULL,
                price DECIMAL NOT NULL,
                stock INT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS accessories.orders (
                id UUID PRIMARY KEY,
                item_id UUID NOT NULL REFERENCES accessories.items(id),
                quantity INT NOT NULL,
                status VARCHAR NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL
            );

         
            """);
    }
}
