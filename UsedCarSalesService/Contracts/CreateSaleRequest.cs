namespace UsedCarSalesService.Contracts;

public class CreateSaleRequest
{
    public Guid CarId { get; set; }
    public Guid ClientId { get; set; }
    public Guid StaffId { get; set; }
    public decimal SalePrice { get; set; }
    public Guid? TradeInId { get; set; }
}
