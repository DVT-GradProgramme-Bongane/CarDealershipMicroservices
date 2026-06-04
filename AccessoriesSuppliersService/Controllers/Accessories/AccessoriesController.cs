using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccessoriesSuppliersService.Features.Accessories;

[ApiController]
[Route("accessories")]
public sealed class AccessoriesController(AccessoriesDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccessoryItemDto>>> List(CancellationToken cancellationToken)
    {
        var accessories = await db.Items
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new AccessoryItemDto(item.Id, item.SupplierId, item.Name, item.Price, item.Stock))
            .ToListAsync(cancellationToken);

        return Ok(accessories);
    }

    [HttpPost]
    public async Task<ActionResult<AccessoryItemDto>> Create(
        CreateAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SupplierId == Guid.Empty)
        {
            return BadRequest(new { error = "SupplierId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Accessory name is required." });
        }

        if (request.Price < 0)
        {
            return BadRequest(new { error = "Price cannot be negative." });
        }

        if (request.Stock < 0)
        {
            return BadRequest(new { error = "Stock cannot be negative." });
        }

        var supplierExists = await db.Suppliers
            .AnyAsync(supplier => supplier.Id == request.SupplierId, cancellationToken);
        if (!supplierExists)
        {
            return NotFound(new { error = "Supplier not found." });
        }

        var item = new AccessoryItem
        {
            Id = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            Name = request.Name.Trim(),
            Price = request.Price,
            Stock = request.Stock
        };

        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new AccessoryItemDto(item.Id, item.SupplierId, item.Name, item.Price, item.Stock);
        return Created($"/accessories/{item.Id}", dto);
    }
}

public sealed record CreateAccessoryRequest(Guid SupplierId, string Name, decimal Price, int Stock);
public sealed record AccessoryItemDto(Guid Id, Guid SupplierId, string Name, decimal Price, int Stock);
