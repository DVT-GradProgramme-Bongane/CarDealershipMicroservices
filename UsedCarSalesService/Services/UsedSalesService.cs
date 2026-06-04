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
    private readonly ILogger<UsedSalesService> _logger;

    public UsedSalesService(
        UsedCarSalesDbContext db,
        InventoryGrpcClient inventory,
        EventBus eventBus,
        ILogger<UsedSalesService> logger)
    {
        _db = db;
        _inventory = inventory;
        _eventBus = eventBus;
        _logger = logger;
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

        var inventoryReserved = false;
        // Used to check if sale was actually written to database
        var salePersisted = false;
        try
        {
            await ExecuteWithRetryAsync(
                () => _inventory.ReserveCarAsync(req.CarId),
                "reserve inventory vehicle");
            inventoryReserved = true;
            _db.Transactions.Add(sale);
            await _db.SaveChangesAsync();
            salePersisted = true;

            await ExecuteWithRetryAsync(
                () => _eventBus.PublishAsync("sale.used.created", new
                {
                    sale_id = sale.Id,
                    car_id = sale.CarId,
                    client_id = sale.ClientId
                }),
                "publish sale.used.created event");

            return sale.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sale creation sync failed for sale {SaleId}; starting compensation.", sale.Id);
            if (salePersisted)
            {
                try
                {
                    _db.Transactions.Remove(sale);
                    await _db.SaveChangesAsync();
                }
                catch (Exception dbRollbackEx)
                {
                    _logger.LogError(dbRollbackEx, "Sale rollback failed for sale {SaleId}.", sale.Id);
                }
            }
            if (inventoryReserved)
            {
                try
                {
                    await ExecuteWithRetryAsync(
                        () => _inventory.UpdateCarStatusAsync(req.CarId, "available"),
                        "rollback reserved inventory vehicle");
                }
                catch (Exception inventoryRollbackEx)
                {
                    _logger.LogError(inventoryRollbackEx, "Inventory rollback failed for sale {SaleId}.", sale.Id);
                }
            }

            throw new InvalidOperationException("Could not complete sale creation due to downstream sync failure.", ex);
        }
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

        var previousStatus = sale.Status;
        sale.Status = normalizedStatus;
        await _db.SaveChangesAsync();

        try
        {
            await SyncStatusSideEffectsAsync(sale, previousStatus, normalizedStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Status sync failed for sale {SaleId}; reverting local status.", sale.Id);

            sale.Status = previousStatus;
            await _db.SaveChangesAsync();

            var rollbackInventoryStatus = string.Equals(previousStatus, "cancelled", StringComparison.OrdinalIgnoreCase)
                ? "available"
                : "reserved";

            try
            {
                await ExecuteWithRetryAsync(
                    () => _inventory.UpdateCarStatusAsync(sale.CarId, rollbackInventoryStatus),
                    "rollback inventory status");
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Inventory status rollback failed for sale {SaleId}.", sale.Id);
            }

            throw new InvalidOperationException("Could not update sale status due to downstream sync failure.", ex);
        }

        return sale;
    }

    private async Task SyncStatusSideEffectsAsync(UsedSalesTransaction sale, string previousStatus, string newStatus)
    {
        if (string.Equals(newStatus, "completed", StringComparison.OrdinalIgnoreCase))
        {
            await ExecuteWithRetryAsync(
                () => _inventory.UpdateCarStatusAsync(sale.CarId, "sold"),
                "mark inventory vehicle as sold");

            await ExecuteWithRetryAsync(
                () => _eventBus.PublishAsync("sale.used.completed", new
                {
                    sale_id = sale.Id,
                    car_id = sale.CarId,
                    client_id = sale.ClientId
                }),
                "publish sale.used.completed event");

            return;
        }

        if (string.Equals(newStatus, "cancelled", StringComparison.OrdinalIgnoreCase))
        {
            await ExecuteWithRetryAsync(
                () => _inventory.UpdateCarStatusAsync(sale.CarId, "available"),
                "release inventory vehicle");

            await ExecuteWithRetryAsync(
                () => _eventBus.PublishAsync("sale.used.cancelled", new
                {
                    sale_id = sale.Id,
                    car_id = sale.CarId,
                    client_id = sale.ClientId
                }),
                "publish sale.used.cancelled event");

            return;
        }

        await ExecuteWithRetryAsync(
            () => _eventBus.PublishAsync("sale.used.status-updated", new
            {
                sale_id = sale.Id,
                old_status = previousStatus,
                new_status = sale.Status
            }),
            "publish sale.used.status-updated event");
    }

    private async Task ExecuteWithRetryAsync(Func<Task> action, string operationName, int maxAttempts = 3)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(
                    ex,
                    "Attempt {Attempt}/{MaxAttempts} failed for operation: {OperationName}.",
                    attempt,
                    maxAttempts,
                    operationName);

                if (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt));
                }
            }
        }

        throw new InvalidOperationException(
            $"Operation '{operationName}' failed after {maxAttempts} attempts.",
            lastException);
    }
}
