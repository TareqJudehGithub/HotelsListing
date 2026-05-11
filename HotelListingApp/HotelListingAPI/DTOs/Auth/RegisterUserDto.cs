using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Auth;

public class RegisterUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(length: 12, ErrorMessage = "{0} max length is {1} characters.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(otherProperty: "Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(length: 12, ErrorMessage = "{0} max length is {1} characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(length: 12, ErrorMessage = "{0} max length is {1} characters.")]
    public string LastName { get; set; } = string.Empty;

    // Default Role value upon registration (In case no value were provided)
    public string Role { get; set; } = "User";

    public int? AssociatedHotelId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Role == "Hotel Admin" && AssociatedHotelId.GetValueOrDefault() < 1)
        {
            yield return new ValidationResult(
                "Please provide a valid Hotel Id",
                [nameof(AssociatedHotelId)]);
        }
    }
}
