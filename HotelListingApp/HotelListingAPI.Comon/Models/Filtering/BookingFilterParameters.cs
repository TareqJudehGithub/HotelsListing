using HotelListingAPI.Common.Enums;

namespace HotelListingAPI.Common.Models.Filtering;

// Filters for bookings
public class BookingFilterParameters : BasedFilterParameters
{
    public BookingStatusEnum? Status { get; set; }
    public DateOnly? CheckInFrom { get; set; }
    public DateOnly? CheckInTo { get; set; }
    public DateOnly? CheckOutFrom { get; set; }
    public DateOnly? CheckOutTo { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinGuests { get; set; }
    public int? MaxGuests { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string? HotelName { get; set; }
}
