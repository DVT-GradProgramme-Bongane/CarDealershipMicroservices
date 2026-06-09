using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Api.Models;

public enum CarType   { New, Used }
public enum CarStatus { Available, Sold, Reserved, InService }

[Table("cars", Schema = "inventory")]
public class Car
{
    [Key]
    [Column(name:"id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(17)]
    [Column("vin")]
    public string Vin { get; set; } = "";
    
    [Column("make")]
    public string Make  { get; set; } = "";
    
    [Column("model")]
    public string Model { get; set; } = "";
    
    [Column("year")]
    public int Year  { get; set; }
    
    [Column("color")]
    public string Color { get; set; } = "";

    [Column(name:"price", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Column("mileage")]
    public int Mileage { get; set; }

    [Column("type")]
    public CarType Type   { get; set; } = CarType.New;
    
    [Column("status")]
    public CarStatus Status { get; set; } = CarStatus.Available;

    [Column("created_at")]
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}