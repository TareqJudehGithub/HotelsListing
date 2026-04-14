using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Data
{
    [Table("Countries", Schema = "dbo")]
    public class Country
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Country Name")]
        [Length(minimumLength: 3, maximumLength: 12, ErrorMessage = "Country name must be between {1} and {2} characters.")]
        public required string Name { get; set; }


        [Required]
        [Display(Name = "Country Short Name")]
        [Length(minimumLength: 2, maximumLength: 3, ErrorMessage = " Short Country Name must be between {1} and {2} characters.")]
        public required string ShortName { get; set; }


        // Navigation property to represent the relationship with hotels
        // One country can have many hotels, but each hotel belongs to one country.
        public IList<Hotel> Hotels { get; set; } = [];

    }
}

