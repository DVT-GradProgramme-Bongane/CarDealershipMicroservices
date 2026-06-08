using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaintenanceService.Models;

[Table("jobs", Schema = "maintenance")]
public class MaintenanceJob
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("car_id")]
    public Guid CarId { get; set; }

    [Column("client_id")]
    public Guid ClientId { get; set; }

    [Column("staff_id")]
    public Guid StaffId { get; set; }

    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("status")]
    public string Status { get; set; } = MaintenanceJobStatuses.Scheduled;

    [Column("scheduled")]
    public DateTime Scheduled { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
