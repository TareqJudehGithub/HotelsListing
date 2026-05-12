using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Application.DTOs.Booking;

public class CreateBookingDto : IValidatableObject
{
    #region Properties    
    [Required]
    public required int HotelId { get; set; }

    [Required]
    [Range(minimum: 1, maximum: 10, ErrorMessage = "Number of guests should be between {1} and {2}")]
    public int Guests { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }
    public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
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


