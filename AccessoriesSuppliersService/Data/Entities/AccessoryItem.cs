using System;
using System.Collections.Generic;

namespace AccessoriesSuppliersService.Data.Entities;

public sealed class AccessoryItem
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Supplier? Supplier { get; set; }
    public List<AccessoryOrder> Orders { get; set; } = [];
}
