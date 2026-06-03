namespace UsedCarSalesService.Models;

public class UsedSalesTransaction
{
    public Guid Id { get; set; }
    public Guid CarId { get; set; }
    public Guid ClientId { get; set; }
    public Guid StaffId { get; set; }
    public decimal SalePrice { get; set; }
    public Guid? TradeInId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
