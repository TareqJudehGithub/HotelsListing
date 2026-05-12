using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Application.DTOs.Booking
{
    public class GetBookingDto
    {
        #region Properties
        public int Id { get; set; }
        public required int HotelId { get; set; }
        public required string HotelName { get; set; }
        public DateOnly CheckIn { get; set; }
        public DateOnly CheckOut { get; set; }
        public int Guests { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdateAtUtc { get; set; }
        #endregion
    }
}
