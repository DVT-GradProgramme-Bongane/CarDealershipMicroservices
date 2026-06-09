using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Data.Entities;
using AccessoriesSuppliersService.Features.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AccessoriesSuppliersService.Tests;

public class SuppliersControllerTests
{
    private AccessoriesDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AccessoriesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AccessoriesDbContext(options);
    }

    [Fact]
    public async Task List_ReturnsSuppliersOrderedByName()
    {
        // Arrange
        using var db = GetDbContext();
        db.Suppliers.AddRange(
            new Supplier { Id = Guid.NewGuid(), Name = "Z Supplier", Contact = "111", Email = "z@s.com" },
            new Supplier { Id = Guid.NewGuid(), Name = "A Supplier", Contact = "222", Email = "a@s.com" }
        );
        await db.SaveChangesAsync();

        var controller = new SuppliersController(db);

        // Act
        var result = await controller.List(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var suppliers = Assert.IsAssignableFrom<IReadOnlyList<SupplierDto>>(okResult.Value);
        Assert.Equal(2, suppliers.Count);
        Assert.Equal("A Supplier", suppliers[0].Name);
        Assert.Equal("Z Supplier", suppliers[1].Name);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new SuppliersController(db);
        var request = new CreateSupplierRequest("New Supplier", "Phone 123", "supplier@test.com");

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<SupplierDto>(createdResult.Value);
        Assert.Equal("New Supplier", dto.Name);
        Assert.Equal("Phone 123", dto.Contact);
        Assert.Equal("supplier@test.com", dto.Email);

        var dbSupplier = await db.Suppliers.FirstOrDefaultAsync(s => s.Id == dto.Id);
        Assert.NotNull(dbSupplier);
        Assert.Equal("New Supplier", dbSupplier.Name);
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var controller = new SuppliersController(db);
        var request = new CreateSupplierRequest("  ", "Phone", "email");

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
