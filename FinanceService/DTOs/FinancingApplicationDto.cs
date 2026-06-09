namespace FinanceService.DTOs;
public record FinancingApplicationDto(
    Guid Id,
    Guid SaleId,
    Guid ClientId,
    decimal LoanAmount,
    decimal InterestRate,
    int TermMonths,
    decimal MonthlyPayment,
    string Status,
    DateTime CreatedAt
);