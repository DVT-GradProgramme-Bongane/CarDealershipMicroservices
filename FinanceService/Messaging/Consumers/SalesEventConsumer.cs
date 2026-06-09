public class SaleEventConsumer
{
    private readonly IFinancingApplicationService _service;

    public SaleEventConsumer(IFinancingApplicationService service)
    {
        _service = service;
    }

    public async Task HandleAsync(SaleCreatedMessage message, CancellationToken ct)
    {
        await _service.CreateFromSaleEventAsync(message.Data.SaleId, message.Data.ClientId, ct);
    }
}