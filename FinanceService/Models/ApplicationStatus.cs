namespace FinanceService.Models;
public static class ApplicationStatus
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";

    public static readonly string[] Valid = [Pending, Approved, Rejected];
}