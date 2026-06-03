using System;
using System.Collections.Generic;

namespace AccessoriesSuppliersService.Data.Entities;

public sealed class Supplier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<AccessoryItem> Items { get; set; } = [];
}
