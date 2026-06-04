using System.ComponentModel.DataAnnotations;

namespace UsedCarSalesService.Contracts;

public class UpdateSaleStatusRequest
{
    [Required]
    [RegularExpression("(?i)^(pending|approved|completed|cancelled)$")]
    public string Status { get; set; } = string.Empty;
}
