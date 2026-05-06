using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.DTOs.Booking
{
    public class CreateBookingDto
    {
        #region Properties    
        public required int HotelId { get; set; }
        public int Guests { get; set; }
        public DateOnly CheckIn { get; set; }
        public DateOnly CheckOut { get; set; }
        #endregion
    }
}
