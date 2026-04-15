using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Data
{
    [Table("Countries", Schema = "dbo")]
    public class Country
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ShortName { get; set; }

        // Navigation property to represent the relationship with hotels
        // One country can have many hotels, but each hotel belongs to one country.
        public IList<Hotel> Hotels { get; set; } = [];

    }
}

