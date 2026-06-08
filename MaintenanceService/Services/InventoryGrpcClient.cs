using Grpc.Net.Client;

namespace MaintenanceService.Services;

public class InventoryGrpcClient
{
    private readonly InventoryService.InventoryServiceClient _client;

    public InventoryGrpcClient(IConfiguration configuration)
    {
        var inventoryUrl = configuration["INVENTORY_GRPC_URL"] ?? "http://localhost:5001";
        var channel = GrpcChannel.ForAddress(inventoryUrl);
        _client = new InventoryService.InventoryServiceClient(channel);
    }

    public async Task MarkCarInServiceAsync(Guid carId)
    {
        await UpdateCarStatusAsync(carId, "InService");
    }

    public async Task MarkCarAvailableAsync(Guid carId)
    {
        await UpdateCarStatusAsync(carId, "available");
    }

    private async Task UpdateCarStatusAsync(Guid carId, string status)
    {
        await _client.UpdateCarStatusAsync(new UpdateStatusRequest
        {
            Id = carId.ToString(),
            Status = status
        });
    }
}
