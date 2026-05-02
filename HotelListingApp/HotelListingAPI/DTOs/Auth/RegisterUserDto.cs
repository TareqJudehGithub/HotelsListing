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
}
