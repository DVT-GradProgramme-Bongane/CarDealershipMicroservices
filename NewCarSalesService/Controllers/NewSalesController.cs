using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewCarSalesService.Models;
using NewCarSalesService.Messaging;
using Grpc.Net.Client;

namespace NewCarSalesService.Controllers;

[ApiController]
[Route("new-sales")]
public class NewSalesController : ControllerBase
{
    private readonly SalesDbContext _context;
    private readonly EventPublisher _publisher;
    private readonly InventoryService.InventoryServiceClient _grpcClient;

    public NewSalesController(SalesDbContext context, EventPublisher publisher, IConfiguration config)
    {
        _context = context;
        _publisher = publisher;

        var inventoryUrl = config["INVENTORY_GRPC_URL"] ?? "http://localhost:5001";
        var channel = GrpcChannel.ForAddress(inventoryUrl);
        _grpcClient = new InventoryService.InventoryServiceClient(channel);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.Transactions.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tx = await _context.Transactions.FindAsync(id);
        return tx == null ? NotFound() : Ok(tx);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NewSales newSales)
    {
        _context.Transactions.Add(newSales);
        await _context.SaveChangesAsync();

        try
        {
            var grpcResponse = await _grpcClient.UpdateCarStatusAsync(new UpdateStatusRequest
            {
                Id = newSales.CarId.ToString(),
                Status = "reserved"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"gRPC Communication Failed: {ex.Message}");
        }

        await _publisher.PublishSaleEventAsync("sale.new.created", newSales.Id, newSales.CarId, newSales.ClientId);
        return CreatedAtAction(nameof(GetById), new { id = newSales.Id }, newSales);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        var tx = await _context.Transactions.FindAsync(id);
        if (tx == null) return NotFound();

        tx.Status = status;
        await _context.SaveChangesAsync();

        if (status.ToLower() == "completed")
        {
            try
            {
                await _grpcClient.UpdateCarStatusAsync(new UpdateStatusRequest
                {
                    Id = tx.CarId.ToString(),
                    Status = "sold"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"gRPC Failed: {ex.Message}");
            }
            await _publisher.PublishSaleEventAsync("sale.new.completed", tx.Id, tx.CarId, tx.ClientId);
        }

        return Ok(tx);
    }
}