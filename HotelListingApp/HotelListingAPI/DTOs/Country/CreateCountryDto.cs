using HotelListingAPI.DTOs.Hotel;
using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Country
{
    public class CreateCountryDto
    {
        [Required]
        [Display(Name = "Country Name")]
        [Length(minimumLength: 3, maximumLength: 12, ErrorMessage = "Country name must be between {1} and {2} characters.")]
        public required string Name { get; set; }

        [Required]
        [Display(Name = "Country Short Name")]
        [Length(minimumLength: 2, maximumLength: 3, ErrorMessage = " Short Country Name must be between {1} and {2} characters.")]
        public required string ShortName { get; set; }

    }

}
