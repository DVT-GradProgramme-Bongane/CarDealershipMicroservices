using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Data.Entities;
using AccessoriesSuppliersService.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccessoriesSuppliersService.Features.Orders;

[ApiController]
[Route("accessories")]
public sealed class AccessoryOrdersController(AccessoriesDbContext db, IRabbitMqPublisher publisher) : ControllerBase
{
    [HttpPost("order")]
    public async Task<ActionResult<AccessoryOrderDto>> Place(
        PlaceAccessoryOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ItemId == Guid.Empty)
        {
            return BadRequest(new { error = "ItemId is required." });
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(new { error = "Quantity must be greater than zero." });
        }

        var item = await db.Items.FirstOrDefaultAsync(
            accessory => accessory.Id == request.ItemId,
            cancellationToken);
        if (item is null)
        {
            return NotFound(new { error = "Accessory item not found." });
        }

        if (item.Stock < request.Quantity)
        {
            return BadRequest(new { error = "Insufficient stock." });
        }

        var previousStock = item.Stock;
        item.Stock -= request.Quantity;

        var order = new AccessoryOrder
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            Quantity = request.Quantity,
            Status = "ordered",
            CreatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync("accessory.order.placed", new
        {
            orderId = order.Id,
            itemId = order.ItemId,
            order.Quantity,
            order.Status,
            order.CreatedAt,
            remainingStock = item.Stock
        }, cancellationToken);

        if (previousStock >= 5 && item.Stock < 5)
        {
            await publisher.PublishAsync("accessory.stock.low", new
            {
                itemId = item.Id,
                item.Name,
                stock = item.Stock
            }, cancellationToken);
        }

        var dto = new AccessoryOrderDto(order.Id, order.ItemId, order.Quantity, order.Status, order.CreatedAt);
        return Created($"/accessories/orders/{order.Id}", dto);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<AccessoryOrderDto>>> List(CancellationToken cancellationToken)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new AccessoryOrderDto(order.Id, order.ItemId, order.Quantity, order.Status, order.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }
}

public sealed record PlaceAccessoryOrderRequest(Guid ItemId, int Quantity);
public sealed record AccessoryOrderDto(Guid Id, Guid ItemId, int Quantity, string Status, DateTime CreatedAt);
