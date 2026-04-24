using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Data
{
    [Table("Hotels", Schema = "dbo")]
    public class Hotel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Address { get; set; }
        public double Rating { get; set; }
        // The CountryId property is a foreign key that links to the Country entity.        
        public int CountryId { get; set; }
        // Navigation property to represent the relationship with the country.
        // One country can have many hotels, but each hotel belongs to one country.
        public required Country Country { get; set; }
    }
}

