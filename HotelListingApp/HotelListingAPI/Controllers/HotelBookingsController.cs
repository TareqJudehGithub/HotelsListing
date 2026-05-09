// Ignore Spelling: Dto
using HotelListingAPI.Contracts;
using HotelListingAPI.Controllers;
using HotelListingAPI.DTOs.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



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
    [Route("{id: int}")]
    public async Task<ActionResult<IEnumerable<GetBookingDto>>> GetHotelBookings(
        [FromRoute] int id
        )
    {
        var result = await _bookingService.GetHotelBookingsAsync(id);
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
    [Route("{bookingId: int}")]
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

    #endregion
}
