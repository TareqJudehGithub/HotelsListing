using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Booking;

public class UpdateBookingDto : IValidatableObject
{
    #region Properties    
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    [Required]
    [Range(minimum: 1, maximum: 10, ErrorMessage = "Number of guests should be between {1} and {2}")]
    public int Guests { get; set; }
    #endregion
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckOut <= CheckIn)
        {
            yield return new ValidationResult(
                errorMessage: "Check-out date must be greater than Check-In date.",
                memberNames: [nameof(CheckOut), nameof(CheckIn)]);
        }
    }
}
