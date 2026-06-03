using System.ComponentModel.DataAnnotations;

namespace UsedCarSalesService.Contracts;

public class CreateSaleRequest : IValidatableObject
{
    [Required]
    public Guid CarId { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [Required]
    public Guid StaffId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal SalePrice { get; set; }

    public Guid? TradeInId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CarId == Guid.Empty)
        {
            yield return new ValidationResult("CarId is required.", new[] { nameof(CarId) });
        }

        if (ClientId == Guid.Empty)
        {
            yield return new ValidationResult("ClientId is required.", new[] { nameof(ClientId) });
        }

        if (StaffId == Guid.Empty)
        {
            yield return new ValidationResult("StaffId is required.", new[] { nameof(StaffId) });
        }

        if (TradeInId.HasValue && TradeInId.Value == Guid.Empty)
        {
            yield return new ValidationResult("TradeInId cannot be an empty GUID.", new[] { nameof(TradeInId) });
        }
    }
}
