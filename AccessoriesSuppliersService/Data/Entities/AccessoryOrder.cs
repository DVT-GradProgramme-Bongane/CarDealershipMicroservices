using System;

namespace AccessoriesSuppliersService.Data.Entities;

public sealed class AccessoryOrder
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "ordered";
    public DateTime CreatedAt { get; set; }
    public AccessoryItem? Item { get; set; }
}
