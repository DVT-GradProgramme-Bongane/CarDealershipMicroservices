public class FinancingApplication
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid ClientId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths {get; set;}
    public decimal MonthlyPayment { get; set; }
    public string Status { get; set; } = ApplicationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public FinancingApplicationDto ToDto() => new(Id, SaleId, ClientId, LoanAmount, InterestRate, TermMonths, MonthlyPayment, Status, CreatedAt);

}