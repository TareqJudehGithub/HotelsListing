using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

using HotelListingAPI.Constants;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Booking;
using HotelListingAPI.Results;
using HotelListingAPI.Data.Enums;


namespace HotelListingAPI.Services;

public class BookingService : IBookingService
{
    #region Fields
    private readonly HotelListingsDbContext _context;
    private readonly IHttpContextAccessor _contextAccessor;
    #endregion

    #region Constructor
    public BookingService(
        HotelListingsDbContext context,
        IHttpContextAccessor contextAccessor)
    {
        _context = context;
        _contextAccessor = contextAccessor;
    }

    #endregion

    #region Methods
    public async Task<Result<IEnumerable<GetBookingDto>>> GetHotelBookingsAsync(int hotelId)
    {
        // Check for existing hotel
        var hotel = await _context.Hotels.FirstOrDefaultAsync(q => q.Id == hotelId);
        if (hotel is null)
        {
            return Result<IEnumerable<GetBookingDto>>
                .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel {hotelId} not found."));
        }
        // Return all bookings for hotel found
        var bookings = await _context.Bookings
            .Where(q => q.HotelId == hotelId)
            .OrderBy(q => q.CheckIn)
            .Select(q => new GetBookingDto
            {
                Id = q.Id,
                HotelId = q.HotelId,
                HotelName = q.Hotel!.Name,
                CheckIn = q.CheckIn,
                CheckOut = q.CheckOut,
                Guests = q.Guests,
                TotalPrice = q.TotalPrice,
                Status = q.Status.ToString(),
                CreateAtUtc = q.CreateAtUtc,
                UpdateAtUtc = q.UpdateAtUtc,
            })
            .ToListAsync();

        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateHotelBookingAsync(CreateBookingDto createBookingDto)
    {

        // Get userId
        // Get userId from JWT claims - Find the 1st name with this sub
        var userId = _contextAccessor?.HttpContext?.User?
             .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        // Check if userId is not null
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>
               .Failure(new Error(Code: ErrorCodes.Validation, Description: "User is required"));
        }

        #region Validations
        // Check if CheckOut Date is greater than CheckIn Date
        var nights = createBookingDto.CheckOut.DayNumber - createBookingDto.CheckIn.DayNumber;
        if (nights <= 0)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Validation, Description: "Check Out Date should be greater " +
                "than Check In Date."));
        }
        if (createBookingDto.Guests <= 0)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Validation, Description: "Number of guest should be 1 or more."));
        }
        var hotel = await _context.Hotels.FirstOrDefaultAsync(q => q.Id == createBookingDto.HotelId);

        // Trevor's
        //var hotel = await _context.Hotels
        //    .Where(q => q.Id == createBookingDto.HotelId)
        //    .FirstOrDefaultAsync();

        // Check if Hotel is not found
        if (hotel is null)
        {
            return Result<GetBookingDto>
               .NotFound(new Error(Code: ErrorCodes.NotFound, Description: "Hotel not found."));
        }
        // Check for overlaps booking from the same user
        // Overlaps means that the new booking's check-in date is before an existing booking's check-out date
        var overlaps = await _context.Bookings
            .AnyAsync(
            q => q.HotelId == createBookingDto.HotelId
           && q.Status != BookingStatusEnum.Cancelled
           && createBookingDto.CheckIn < createBookingDto.CheckOut
           && createBookingDto.CheckOut > createBookingDto.CheckIn
           && q.UserId == userId
            );

        if (overlaps)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Conflict, Description: "The selected dates overlap with an existing booking."));
        }
        #endregion

        var totalPrice = hotel.PerNightRate * nights;

        // Map to Domain Model
        var booking = new Booking
        {
            HotelId = createBookingDto.HotelId,
            UserId = userId,
            Guests = createBookingDto.Guests,
            CheckIn = createBookingDto.CheckIn,
            CheckOut = createBookingDto.CheckOut,
            Status = BookingStatusEnum.pending,
            TotalPrice = totalPrice,
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Map to DTO Model
        var createdBooking = new GetBookingDto
        {
            Id = booking.Id,
            HotelId = hotel.Id,
            HotelName = hotel.Name,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Guests = booking.Guests,
            TotalPrice = booking.TotalPrice,
            // Trevor's TotalPrice: TotalPrice = totalPrice
            Status = booking.Status.ToString(),
            CreateAtUtc = booking.CreateAtUtc,
            UpdateAtUtc = booking.UpdateAtUtc
        };

        return Result<GetBookingDto>.Success(createdBooking);
    }

    #endregion
}

