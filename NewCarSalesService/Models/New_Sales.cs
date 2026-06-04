using System.ComponentModel.DataAnnotations.Schema;
namespace NewCarSalesService.Models;


[Table("transactions", Schema = "new_sales")]
public class NewSales
{
    [Column("id")]
    public Guid Id { get; set; } = new Guid();

    [Column("car_id")]
    public Guid CarId { get; set; }

    [Column("client_id")]
    public Guid ClientId { get; set; }

    [Column("staff_id")]
    public Guid StaffId { get; set; }

    [Column("sales_price")]
    public decimal SalesPrice { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}