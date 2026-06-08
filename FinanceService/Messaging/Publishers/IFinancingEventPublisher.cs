namespace FinanceService.Publishers;
public interface IFinancingEventPublisher
{
    Task PublishApprovedAsync(Guid applicationId, Guid saleId, Guid clientId, CancellationToken ct);
    Task PublishRejectedAsync(Guid applicationId, Guid saleId, Guid clientId, CancellationToken ct);
}