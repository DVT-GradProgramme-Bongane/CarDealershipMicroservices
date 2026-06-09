using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Data.Entities;
using AccessoriesSuppliersService.Features.Accessories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AccessoriesSuppliersService.Tests;

public class AccessoriesControllerTests
{
    private AccessoriesDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AccessoriesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AccessoriesDbContext(options);
    }

    [Fact]
    public async Task List_ReturnsAccessoriesOrderedByName()
    {
        // Arrange
        using var db = GetDbContext();
        var supplierId = Guid.NewGuid();
        db.Items.AddRange(
            new AccessoryItem { Id = Guid.NewGuid(), SupplierId = supplierId, Name = "Z Item", Price = 10, Stock = 5 },
            new AccessoryItem { Id = Guid.NewGuid(), SupplierId = supplierId, Name = "A Item", Price = 20, Stock = 10 }
        );
        await db.SaveChangesAsync();

        var controller = new AccessoriesController(db);

        // Act
        var result = await controller.List(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var accessories = Assert.IsAssignableFrom<IReadOnlyList<AccessoryItemDto>>(okResult.Value);
        Assert.Equal(2, accessories.Count);
        Assert.Equal("A Item", accessories[0].Name);
        Assert.Equal("Z Item", accessories[1].Name);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        using var db = GetDbContext();
        var supplierId = Guid.NewGuid();
        var supplier = new Supplier { Id = supplierId, Name = "Test Supplier", Contact = "123", Email = "test@supplier.com" };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        var controller = new AccessoriesController(db);
        var request = new CreateAccessoryRequest(supplierId, "New Accessory", 15.50m, 100);

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<AccessoryItemDto>(createdResult.Value);
        Assert.Equal("New Accessory", dto.Name);
        Assert.Equal(15.50m, dto.Price);
        Assert.Equal(100, dto.Stock);
        Assert.Equal(supplierId, dto.SupplierId);

        var dbItem = await db.Items.FirstOrDefaultAsync(i => i.Id == dto.Id);
        Assert.NotNull(dbItem);
        Assert.Equal("New Accessory", dbItem.Name);
    }

    [Fact]
    public async Task Create_WithEmptySupplierId_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new AccessoriesController(db);
        var request = new CreateAccessoryRequest(Guid.Empty, "Accessory Name", 10m, 5);

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new AccessoriesController(db);
        var request = new CreateAccessoryRequest(Guid.NewGuid(), " ", 10m, 5);

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithNegativePrice_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new AccessoriesController(db);
        var request = new CreateAccessoryRequest(Guid.NewGuid(), "Name", -5m, 5);

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithNegativeStock_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new AccessoriesController(db);
        var request = new CreateAccessoryRequest(Guid.NewGuid(), "Name", 10m, -1);

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithNonExistentSupplier_ReturnsNotFound()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new AccessoriesController(db);
        var request = new CreateAccessoryRequest(Guid.NewGuid(), "Name", 10m, 5);

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
