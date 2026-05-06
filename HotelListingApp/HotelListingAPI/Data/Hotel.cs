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

        [Column(TypeName = "decimal(18, 3)")]
        public decimal PerNightRate { get; set; }


        // The CountryId property is a foreign key that links to the Country entity.        
        public int CountryId { get; set; }

        // Navigation property to represent the relationship with the country.
        // One country can have many hotels, but each hotel belongs to one country.
        public required Country Country { get; set; }


        // A collection of HotelAdmin object

        // A hotel can have multiple admins, and each admin can manage multiple hotels, so this is a many-to-many relationship.
        public ICollection<HotelAdmin> Admins { get; set; } = [];

        // A hotel can have multiple bookings, but each booking is associated with one hotel, so this is a one-to-many relationship.
        public ICollection<Booking> Bookings { get; set; } = [];
    }
}

