using HotelListingAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListingAPI.Domain;

public class Booking
{
    #region Properties
    public int Id { get; set; }
    public int Guests { get; set; }
    public DateOnly CheckIn { get; set; }
    public DateOnly CheckOut { get; set; }

    [Column(TypeName = "decimal(18, 3)")]
    public decimal TotalPrice { get; set; }

    // Booking status, default is pending 
    public BookingStatusEnum Status { get; set; } = BookingStatusEnum.pending;

    // Auditing properties
    public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAtUtc { get; set; }
    #endregion

    #region FKs
    public required int HotelId { get; set; }
    public required string UserId { get; set; } = string.Empty;
    #endregion

    #region Navigation Properties
    public Hotel? Hotel { get; set; }
    public ApplicationUser? User { get; set; }
    #endregion
}

