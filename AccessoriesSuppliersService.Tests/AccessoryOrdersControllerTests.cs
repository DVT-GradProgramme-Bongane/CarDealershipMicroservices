using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Data.Entities;
using AccessoriesSuppliersService.Features.Orders;
using AccessoriesSuppliersService.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AccessoriesSuppliersService.Tests;

public class AccessoryOrdersControllerTests
{
    private AccessoriesDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AccessoriesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AccessoriesDbContext(options);
    }

    [Fact]
    public async Task List_ReturnsOrdersOrderedByDateDescending()
    {
        // Arrange
        using var db = GetDbContext();
        var itemId = Guid.NewGuid();
        var publisher = Substitute.For<IRabbitMqPublisher>();

        var oldOrder = new AccessoryOrder { Id = Guid.NewGuid(), ItemId = itemId, Quantity = 1, Status = "ordered", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
        var newOrder = new AccessoryOrder { Id = Guid.NewGuid(), ItemId = itemId, Quantity = 2, Status = "ordered", CreatedAt = DateTime.UtcNow };

        db.Orders.AddRange(oldOrder, newOrder);
        await db.SaveChangesAsync();

        var controller = new AccessoryOrdersController(db, publisher);

        // Act
        var result = await controller.List(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsAssignableFrom<IReadOnlyList<AccessoryOrderDto>>(okResult.Value);
        Assert.Equal(2, orders.Count);
        Assert.Equal(newOrder.Id, orders[0].Id);
        Assert.Equal(oldOrder.Id, orders[1].Id);
    }

    [Fact]
    public async Task Place_WithValidRequest_DecrementsStockPublishesMessageAndReturnsCreated()
    {
        // Arrange
        using var db = GetDbContext();
        var itemId = Guid.NewGuid();
        var item = new AccessoryItem { Id = itemId, Name = "Screwdriver", Price = 5.99m, Stock = 10 };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IRabbitMqPublisher>();
        var controller = new AccessoryOrdersController(db, publisher);
        var request = new PlaceAccessoryOrderRequest(itemId, 3);

        // Act
        var result = await controller.Place(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var dto = Assert.IsType<AccessoryOrderDto>(createdResult.Value);
        Assert.Equal(itemId, dto.ItemId);
        Assert.Equal(3, dto.Quantity);
        Assert.Equal("ordered", dto.Status);

        // Verify Database
        var dbItem = await db.Items.FindAsync(itemId);
        Assert.NotNull(dbItem);
        Assert.Equal(7, dbItem.Stock);

        var dbOrder = await db.Orders.FirstOrDefaultAsync(o => o.Id == dto.Id);
        Assert.NotNull(dbOrder);
        Assert.Equal(3, dbOrder.Quantity);

        // Verify RabbitMQ publishing
        await publisher.Received(1).PublishAsync(
            "accessory.order.placed",
            Arg.Is<object>(obj => obj.ToString()!.Contains("orderId")),
            Arg.Any<CancellationToken>()
        );

        // Stock did not fall below 5, so no low stock message
        await publisher.DidNotReceive().PublishAsync(
            "accessory.stock.low",
            Arg.Any<object>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Place_WhenStockDropsBelowFive_PublishesLowStockMessage()
    {
        // Arrange
        using var db = GetDbContext();
        var itemId = Guid.NewGuid();
        var item = new AccessoryItem { Id = itemId, Name = "Hammer", Price = 12.99m, Stock = 6 };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IRabbitMqPublisher>();
        var controller = new AccessoryOrdersController(db, publisher);
        var request = new PlaceAccessoryOrderRequest(itemId, 2); // 6 -> 4 (crosses 5 threshold)

        // Act
        var result = await controller.Place(request, CancellationToken.None);

        // Assert
        Assert.IsType<CreatedResult>(result.Result);

        // Verify RabbitMQ publishing for low stock
        await publisher.Received(1).PublishAsync(
            "accessory.stock.low",
            Arg.Is<object>(obj => obj.ToString()!.Contains("itemId") && obj.ToString()!.Contains("Hammer")),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Place_WithEmptyItemId_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var publisher = Substitute.For<IRabbitMqPublisher>();
        var controller = new AccessoryOrdersController(db, publisher);
        var request = new PlaceAccessoryOrderRequest(Guid.Empty, 5);

        // Act
        var result = await controller.Place(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Place_WithNegativeQuantity_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var publisher = Substitute.For<IRabbitMqPublisher>();
        var controller = new AccessoryOrdersController(db, publisher);
        var request = new PlaceAccessoryOrderRequest(Guid.NewGuid(), 0);

        // Act
        var result = await controller.Place(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Place_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        using var db = GetDbContext();
        var publisher = Substitute.For<IRabbitMqPublisher>();
        var controller = new AccessoryOrdersController(db, publisher);
        var request = new PlaceAccessoryOrderRequest(Guid.NewGuid(), 1);

        // Act
        var result = await controller.Place(request, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Place_WithInsufficientStock_ReturnsBadRequest()
    {
        // Arrange
        using var db = GetDbContext();
        var itemId = Guid.NewGuid();
        var item = new AccessoryItem { Id = itemId, Name = "Hammer", Price = 12.99m, Stock = 10 };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IRabbitMqPublisher>();
        var controller = new AccessoryOrdersController(db, publisher);
        var request = new PlaceAccessoryOrderRequest(itemId, 11);

        // Act
        var result = await controller.Place(request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
