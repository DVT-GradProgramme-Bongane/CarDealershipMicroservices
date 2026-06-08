namespace MaintenanceService.Models;

public class CreateMaintenanceJobRequest
{
    [Required]
    public Guid CarId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Scheduled { get; set; }
}
