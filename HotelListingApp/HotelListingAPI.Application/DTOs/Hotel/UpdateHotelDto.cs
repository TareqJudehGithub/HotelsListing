using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Application.DTOs.Hotel
{
    public class UpdateHotelDto : CreateHotelDto
    {
        [Required]
        public int Id { get; set; }
    }
}
