using HotelListingAPI.DTOs.Booking;
using HotelListingAPI.Results;

namespace HotelListingAPI.Contracts
{
    public interface IBookingService
    {
        Task<Result<IEnumerable<GetBookingDto>>> GetHotelBookingsAsync(int id);
        Task<Result<GetBookingDto>> CreateHotelBookingAsync(CreateBookingDto createBookingDto);
        Task<Result<GetBookingDto>> UpdateHotelBookingAsync(
            int hotelId,
            int bookingId,
            UpdateBookingDto updateBookingDto);
        Task<Result> CancelHotelBookingAsync(int hotelId, int bookingId);
    }
}

