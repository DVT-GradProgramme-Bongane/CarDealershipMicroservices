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
                created_at TIMESTAMP NOT NULL
            );

            INSERT INTO accessories.suppliers (id, name, contact, email)
            VALUES
                ('11111111-1111-1111-1111-111111111111', 'AutoStyle Distributors', 'Thabo Mokoena', 'orders@autostyle.example'),
                ('22222222-2222-2222-2222-222222222222', 'RoadReady Accessories', 'Lerato Naidoo', 'sales@roadready.example'),
                ('33333333-3333-3333-3333-333333333333', 'Prime Parts Supply', 'Michael Smith', 'support@primeparts.example')
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO accessories.items (id, supplier_id, name, price, stock)
            VALUES
                ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'All-weather floor mats', 899.99, 18),
                ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '11111111-1111-1111-1111-111111111111', 'Boot liner', 749.50, 4),
                ('cccccccc-cccc-cccc-cccc-cccccccccccc', '22222222-2222-2222-2222-222222222222', 'Tow bar kit', 3299.00, 7),
                ('dddddddd-dddd-dddd-dddd-dddddddddddd', '22222222-2222-2222-2222-222222222222', 'Roof rack set', 2499.00, 12),
                ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '33333333-3333-3333-3333-333333333333', 'Dash camera', 1599.99, 3)
            ON CONFLICT (id) DO NOTHING;

            INSERT INTO accessories.orders (id, item_id, quantity, status, created_at)
            VALUES
                ('99999999-9999-9999-9999-999999999999', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 2, 'ordered', TIMESTAMP '2026-06-01 09:00:00'),
                ('88888888-8888-8888-8888-888888888888', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 1, 'received', TIMESTAMP '2026-06-02 14:30:00')
            ON CONFLICT (id) DO NOTHING;
            """);
    }
}
