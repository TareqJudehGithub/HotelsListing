using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.DTOs.Hotel
{
    public class UpdateHotelDto : CreateHotelDto
    {
        [Required]
        public int Id { get; set; }
    }
}
