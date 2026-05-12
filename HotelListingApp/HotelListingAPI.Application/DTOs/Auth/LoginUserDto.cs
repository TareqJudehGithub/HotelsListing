using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Application.DTOs.Auth;

public class LoginUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}
