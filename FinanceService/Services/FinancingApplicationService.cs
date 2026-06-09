namespace FinanceService.Services;

using Microsoft.EntityFrameworkCore;
using FinanceService.Data;
using FinanceService.DTOs;
using FinanceService.Models;
using FinanceService.Messaging.Publishers;

public class FinancingApplicationService : IFinancingApplicationService
{
    private readonly FinancingDbContext _context;
    private readonly IFinancingEventPublisher _publisher;

    public FinancingApplicationService(FinancingDbContext context, IFinancingEventPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<IEnumerable<FinancingApplicationDto>> GetAllAsync(CancellationToken ct) =>
        await _context.Applications.Select(a => a.ToDto()).ToListAsync(ct);

    public async Task<FinancingApplicationDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var application = await _context.Applications.FindAsync([id], ct);
        return application?.ToDto();
    }

    public async Task<FinancingApplicationDto> CreateAsync(CreateFinancingApplicationDto dto, CancellationToken ct)
    {
        var application = new FinancingApplication
        {
            Id = Guid.NewGuid(),
            SaleId = dto.SaleId,
            ClientId = dto.ClientId,
            LoanAmount = dto.LoanAmount,
            TermMonths = dto.TermMonths,
            MonthlyPayment = dto.TermMonths > 0 ? Math.Round(dto.LoanAmount / dto.TermMonths, 2) : 0,
            Status = ApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
			InterestRate = dto.InterestRate,
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(ct);

        return application.ToDto();
    }

    public async Task<FinancingApplicationDto?> UpdateStatusAsync(Guid id, string status, CancellationToken ct)
    {
        var application = await _context.Applications.FindAsync([id], ct);
        if (application is null) return null;

        application.Status = status;
        await _context.SaveChangesAsync(ct);

        if (status == ApplicationStatus.Approved)
            await _publisher.PublishApprovedAsync(application.Id, application.SaleId, application.ClientId, ct);
        else if (status == ApplicationStatus.Rejected)
            await _publisher.PublishRejectedAsync(application.Id, application.SaleId, application.ClientId, ct);

        return application.ToDto();
    }

    public async Task CreateFromSaleEventAsync(Guid saleId, Guid clientId, CancellationToken ct)
    {
        var application = new FinancingApplication
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ClientId = clientId,
            LoanAmount = 0,
            TermMonths = 0,
            MonthlyPayment = 0,
            Status = ApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(ct);
    }
}