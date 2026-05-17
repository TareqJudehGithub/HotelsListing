using HotelListingAPI.Application.DTOs.Booking;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Common.Models.Filtering;
using HotelListingAPI.Common.Results;

namespace HotelListingAPI.Application.Contracts
{
    public interface IBookingService
    {
        Task<Result<PagedResult<GetBookingDto>>> UserGetHotelBookingsAsync
            (int hotelId,
            PaginationParameters paginationParameters,
            BookingFilterParameters filters
            );
        Task<Result<GetBookingDto>> CreateHotelBookingAsync(CreateBookingDto createBookingDto);
        Task<Result<GetBookingDto>> UpdateHotelBookingAsync(
            int hotelId,
            int bookingId,
            UpdateBookingDto updateBookingDto);
        #region Admin
        Task<Result> CancelHotelBookingAsync(int hotelId, int bookingId);
        Task<Result<PagedResult<GetBookingDto>>> AdminGetHotelBookingsAsync
            (int hotelId,
            PaginationParameters paginationParameters,
            BookingFilterParameters filters);
        Task<Result> AdminCancelHotelBookingAsync(int hotelId, int bookingId);
        Task<Result> AdminConfirmHotelBookingAsync(int hotelId, int bookingId);
        #endregion
    }
}

