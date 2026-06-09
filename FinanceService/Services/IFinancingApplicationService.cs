namespace FinanceService.Services;
using FinanceService.DTOs;
public interface IFinancingApplicationService
{
    Task<IEnumerable<FinancingApplicationDto>> GetAllAsync(CancellationToken ct);
    Task<FinancingApplicationDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<FinancingApplicationDto> CreateAsync(CreateFinancingApplicationDto dto, CancellationToken ct);
    Task<FinancingApplicationDto?> UpdateStatusAsync(Guid id, string status, CancellationToken ct);
    Task CreateFromSaleEventAsync(Guid saleId, Guid clientId, CancellationToken ct);
}