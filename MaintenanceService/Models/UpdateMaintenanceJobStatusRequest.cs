namespace MaintenanceService.Models;

public class UpdateMaintenanceJobStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
