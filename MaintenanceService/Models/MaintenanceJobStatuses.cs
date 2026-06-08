namespace MaintenanceService.Models;

public static class MaintenanceJobStatuses
{
    public const string Scheduled = "scheduled";
    public const string InProgress = "in-progress";
    public const string Completed = "completed";

    public static readonly string[] All =
    [
        Scheduled,
        InProgress,
        Completed
    ];

    public static bool IsValid(string status)
    {
        return All.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}
