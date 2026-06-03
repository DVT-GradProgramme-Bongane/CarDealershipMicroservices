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
    public async Task<ActionResult<IReadOnlyList<Supplier>>> List(CancellationToken cancellationToken)
    {
        var suppliers = await db.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .ToListAsync(cancellationToken);

        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<ActionResult<Supplier>> Create(
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

        return Created($"/suppliers/{supplier.Id}", supplier);
    }
}

public sealed record CreateSupplierRequest(string Name, string Contact, string Email);
