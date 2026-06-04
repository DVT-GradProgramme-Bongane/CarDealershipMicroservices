using AccessoriesSuppliersService.Data;
using AccessoriesSuppliersService.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccessoriesSuppliersService.Features.Suppliers;

[ApiController]
[Route("suppliers")]
public sealed class SuppliersController(AccessoriesDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupplierDto>>> List(CancellationToken cancellationToken)
    {
        var suppliers = await db.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .Select(supplier => new SupplierDto(supplier.Id, supplier.Name, supplier.Contact, supplier.Email))
            .ToListAsync(cancellationToken);

        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(
        CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Supplier name is required." });
        }

        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Contact = request.Contact.Trim(),
            Email = request.Email.Trim()
        };

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new SupplierDto(supplier.Id, supplier.Name, supplier.Contact, supplier.Email);
        return Created($"/suppliers/{supplier.Id}", dto);
    }
}

public sealed record CreateSupplierRequest(string Name, string Contact, string Email);
public sealed record SupplierDto(Guid Id, string Name, string Contact, string Email);
