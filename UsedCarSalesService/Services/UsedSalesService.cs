using Microsoft.EntityFrameworkCore;
using UsedCarSalesService.Contracts;
using UsedCarSalesService.Data;
using UsedCarSalesService.Models;

namespace UsedCarSalesService.Services;

public class UsedSalesService
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "approved",
        "completed",
        "cancelled"
    };

    private readonly UsedCarSalesDbContext _db;
    private readonly InventoryGrpcClient _inventory;
    private readonly EventBus _eventBus;

    public UsedSalesService(
        UsedCarSalesDbContext db,
        InventoryGrpcClient inventory,
        EventBus eventBus)
    {
        _db = db;
        _inventory = inventory;
        _eventBus = eventBus;
    }

    public async Task<IReadOnlyList<UsedSalesTransaction>> GetAll()
        => await _db.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<UsedSalesTransaction?> GetById(Guid id)
        => await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Guid> CreateSale(CreateSaleRequest req)
    {
        var sale = new UsedSalesTransaction
        {
            Id = Guid.NewGuid(),
            CarId = req.CarId,
            ClientId = req.ClientId,
            StaffId = req.StaffId,
            SalePrice = req.SalePrice,
            TradeInId = req.TradeInId,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Transactions.Add(sale);
        await _db.SaveChangesAsync();

        await _inventory.ReserveCarAsync(req.CarId);
        await _eventBus.PublishAsync("sale.used.created", sale);

        return sale.Id;
    }

    public async Task<UsedSalesTransaction?> UpdateStatus(Guid id, string status)
    {
        var sale = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        if (sale is null)
        {
            return null;
        }

        var normalizedStatus = status.Trim().ToLowerInvariant();
        if (!ValidStatuses.Contains(normalizedStatus))
        {
            throw new ArgumentException("Invalid sale status.", nameof(status));
        }

        sale.Status = normalizedStatus;
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync("sale.used.status-updated", new
        {
            sale.Id,
            sale.Status
        });

        return sale;
    }
}
