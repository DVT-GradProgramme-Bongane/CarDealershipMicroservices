using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Api.Models;

public enum CarType   { New, Used }
public enum CarStatus { Available, Sold, Reserved, InService }

public class Car
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(17)]
    public string Vin { get; set; } = "";

    public string Make  { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year  { get; set; }
    public string Color { get; set; } = "";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Mileage { get; set; }

    public CarType Type   { get; set; } = CarType.New;
    public CarStatus Status { get; set; } = CarStatus.Available;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}