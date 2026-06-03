namespace UsedCarSalesService.Services;

public class InventoryGrpcClient
{
    private readonly global::InventoryService.InventoryServiceClient _client;

    public InventoryGrpcClient(global::InventoryService.InventoryServiceClient client)
    {
        _client = client;
    }

    public async Task ReserveCarAsync(Guid carId)
    {
        await UpdateCarStatusAsync(carId, "reserved");
    }

    public async Task UpdateCarStatusAsync(Guid carId, string status)
    {
        await _client.UpdateCarStatusAsync(new global::UpdateStatusRequest
        {
            Id = carId.ToString(),
            Status = status
        });
    }
}
