// Ignore Spelling: Dto

using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Hotel;

public class CreateHotelDto
{
    [Required]
    [Display(Name = "Hotel Name")]
    [MaxLength(50, ErrorMessage = "Hotel Name cannot exceed {1} characters.")]
    [Length(minimumLength: 3, maximumLength: 50, ErrorMessage = "Hotel name must be between {1} and {2} characters.")]
    public required string Name { get; set; }

    [Required]
    [Display(Name = "Hotel Address")]
    [MaxLength(100, ErrorMessage = "Hotel Address cannot exceed {1} characters.")]
    [Length(minimumLength: 3, maximumLength: 100, ErrorMessage = "Hotel name must be between {1} and {2} characters.")]
    public required string Address { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between {1} and {2}.")]
    public double Rating { get; set; }

    [Required(ErrorMessage = "Country Id must not be empty")]
    [Range(minimum: 0, maximum: int.MaxValue, ErrorMessage = "Please enter a valid Country Id number.")]
    public int CountryId { get; set; }
}
