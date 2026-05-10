using HotelListingAPI.DTOs.Booking;
using HotelListingAPI.Results;

namespace HotelListingAPI.Contracts
{
    public interface IBookingService
    {
        Task<Result<IEnumerable<GetBookingDto>>> UserGetHotelBookingsAsync(int hotelId);
        Task<Result<GetBookingDto>> CreateHotelBookingAsync(CreateBookingDto createBookingDto);
        Task<Result<GetBookingDto>> UpdateHotelBookingAsync(
            int hotelId,
            int bookingId,
            UpdateBookingDto updateBookingDto);
        #region Admin
        Task<Result> CancelHotelBookingAsync(int hotelId, int bookingId);
        Task<Result<IEnumerable<GetBookingDto>>> AdminGetHotelBookingsAsync(int hotelId);
        Task<Result> AdminCancelHotelBookingAsync(int hotelId, int bookingId);
        Task<Result> AdminConfirmHotelBookingAsync(int hotelId, int bookingId);
        #endregion
    }
}

