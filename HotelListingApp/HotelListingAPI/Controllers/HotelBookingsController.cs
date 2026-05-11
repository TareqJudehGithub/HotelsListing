// Ignore Spelling: Dto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HotelListingAPI.AuthorizationFilters;
using HotelListingAPI.Contracts;
using HotelListingAPI.DTOs.Booking;

namespace HotelListingAPI.Controllers;

// "https://localhost/api/hotels/{hotelId}/bookings"
[Route("api/hotels/{hotelId:int}/bookings")]
[ApiController]
[Authorize]
public class HotelBookingsController : BaseApiController
{
    #region Fields
    private readonly IBookingService _bookingService;
    #endregion

    #region Constructors
    public HotelBookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }
    #endregion

    #region Methods
    // GET: "https://localhost/api/hotels/{hotelId}/bookings"
    [HttpGet]
    // [Route("{id:int}")]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> UserGetHotelBookings(
        [FromRoute] int hotelId
        )
    {
        var result = await _bookingService.UserGetHotelBookingsAsync(hotelId);
        return ToActionResult(result: result);
    }

    // POST: "https://localhost/api/hotels/{hotelId}/bookings"
    [HttpPost]
    public async Task<ActionResult<GetBookingDto>> CreateHotelBooking(
        [FromBody] CreateBookingDto createBookingDto
        )
    {
        var result = await _bookingService.CreateHotelBookingAsync(createBookingDto);
        return ToActionResult(result: result);
    }

    // PUT: "https://localhost/api/hotels/{hotelId}/bookings/bookingId"
    [HttpPut]
    [Route("{bookingId:int}")]
    //[Route("{bookingId:int}")]
    public async Task<ActionResult<GetBookingDto>> UpdateHotelBooking(
        [FromRoute] int hotelId,
        [FromRoute] int bookingId,
        [FromBody] UpdateBookingDto updateBookingDto)
    {
        var result = await _bookingService.UpdateHotelBookingAsync(
            hotelId, bookingId, updateBookingDto);

        return ToActionResult(result: result);
    }

    // PUT: "https://localhost/api/hotels/{hotelId}/bookings/bookingId/cancel"
    [HttpPut]
    [Route("{bookingId:int}/cancel")]
    public async Task<IActionResult> CancelHotelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await _bookingService.CancelHotelBookingAsync(hotelId, bookingId);
        return ToActionResult(result: result);
    }

    #region HotelAdmin Endpoints   
    // GET: "https://localhost/api/hotels/{hotelId}/bookings/admin"
    [HttpGet]
    [Route("admin")]
    // Test:    [Route("{id: int}/admin")]
    [HotelOrSystemAdmin]
    //[Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> AdminGetHotelBookingsAdmin(
        [FromRoute] int hotelId
        )
    {
        var result = await _bookingService.AdminGetHotelBookingsAsync(hotelId);
        return ToActionResult(result: result);
    }

    // PUT: "https://localhost/api/hotels{hotelId}/bookings/{bookingId}/admin/cancel"
    [HttpPut]
    [Route("{bookingId:int}/admin/cancel")]
    [HotelOrSystemAdmin]
    //[Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<IActionResult> AdminCancelHotelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await _bookingService.AdminConfirmHotelBookingAsync(hotelId, bookingId);
        return ToActionResult(result: result);
    }

    // PUT: "https://localhost/api/hotels{hotelId}/bookings/{bookingId}/admin/confirm"
    [HttpPut]
    [Route("{bookingId:int}/admin/confirm")]
    [HotelOrSystemAdmin]
    //[Authorize(Roles = "Hotel Admin, Administrator")]
    public async Task<IActionResult> AdminConfirmHotelBooking([FromRoute] int hotelId, [FromRoute] int bookingId)
    {
        var result = await _bookingService.AdminConfirmHotelBookingAsync(hotelId, bookingId);
        return ToActionResult(result: result);
    }
    #endregion
    #endregion
}
