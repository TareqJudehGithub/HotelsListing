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
        var userId = _contextAccessor?
            .HttpContext?
            .User?
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
        // Check number of guests validity
        if (createBookingDto.Guests <= 0)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Validation, Description: "Number of guest should be 1 or more."));
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

        var hotel = await _context.Hotels.FirstOrDefaultAsync(q => q.Id == createBookingDto.HotelId);
        #region Trevor's var hotel
        //var hotel = await _context.Hotels
        //    .Where(q => q.Id == createBookingDto.HotelId)
        //    .FirstOrDefaultAsync();
        #endregion

        // Check if Hotel is not found
        if (hotel is null)
        {
            return Result<GetBookingDto>
               .NotFound(new Error(Code: ErrorCodes.NotFound, Description: "Hotel not found."));
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

    public async Task<Result<GetBookingDto>> UpdateHotelBookingAsync(
        int hotelId,
        int bookingId,
        UpdateBookingDto updateBookingDto)
    {
        // Get userId
        // Get userId from JWT claims - Find the 1st name with this sub
        var userId = _contextAccessor?
            .HttpContext?
            .User?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        // Check if userId is not null
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>
               .Failure(new Error(Code: ErrorCodes.Validation, Description: "User is required"));
        }

        #region Validations
        // Check if CheckOut Date is greater than CheckIn Date
        var nights = updateBookingDto.CheckOut.DayNumber - updateBookingDto.CheckIn.DayNumber;
        if (nights <= 0)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Validation, Description: "Check Out Date should be greater " +
                "than Check In Date."));
        }
        // Check number of guests validity
        if (updateBookingDto.Guests <= 0)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Validation, Description: "Number of guest should be 1 or more."));
        }

        // Check for overlaps booking from the same user
        // Overlaps means that the new booking's check-in date is before an existing booking's check-out date
        var overlaps = await _context.Bookings
            .AnyAsync(
            q => q.HotelId == hotelId
           && q.Status != BookingStatusEnum.Cancelled
           && updateBookingDto.CheckIn < updateBookingDto.CheckOut
           && updateBookingDto.CheckOut > updateBookingDto.CheckIn
           && q.UserId == userId
            );

        if (overlaps)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Conflict, Description: "The selected dates overlap with an existing booking."));
        }

        // Check if booking exists
        var booking = await _context.Bookings
            .Include(q => q.Hotel)
            .FirstOrDefaultAsync(q =>
            q.Id == bookingId
            && q.HotelId == hotelId
            && q.UserId == userId);

        if (booking is null)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.NotFound, Description: "Booking was not found."));
        }

        // Check if booking has been cancelled 
        if (booking.Status == BookingStatusEnum.Cancelled)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Conflict, Description: "Cancelled bookings cannot be modified."));
        }
        #endregion

        // Update booking record
        var perNight = booking.Hotel!.PerNightRate;
        booking.CheckIn = updateBookingDto.CheckIn;
        booking.CheckOut = updateBookingDto.CheckOut;
        booking.Guests = updateBookingDto.Guests;
        booking.TotalPrice = perNight * (
            updateBookingDto.CheckOut.DayNumber - updateBookingDto.CheckIn.DayNumber);
        booking.UpdateAtUtc = DateTime.UtcNow;

        // Save changes
        await _context.SaveChangesAsync();

        // Map to Dto
        var updatedBooking = new GetBookingDto
        {
            Id = booking.Id,
            HotelId = booking.HotelId,
            HotelName = booking.Hotel!.Name,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Guests = booking.Guests,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            CreateAtUtc = booking.CreateAtUtc,
            UpdateAtUtc = booking.UpdateAtUtc
        };
        return Result<GetBookingDto>.Success(updatedBooking);
    }

    public async Task<Result> CancelHotelBookingAsync(int hotelId, int bookingId)
    {
        // Get userId 
        var userId = _contextAccessor
             .HttpContext?
             .User?
             .FindFirst(type: JwtRegisteredClaimNames.Sub)?
             .Value;

        // Check if userId exists
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.NotFound, Description: "UserId not found"));
        }

        // Get booking
        var existingBooking = await _context.Bookings
            .Include(q => q.Hotel)
            .FirstOrDefaultAsync(q =>
            q.Id == bookingId
            && q.HotelId == hotelId
            && q.UserId == userId
            );

        if (existingBooking is null)
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.NotFound, Description: "Booking was not found."));
        }

        // Check if booking has been cancelled 
        if (existingBooking.Status == BookingStatusEnum.Cancelled)
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.Conflict, Description: "This booking has already been cancelled."));
        }

        // Update booking status and save
        existingBooking.Status = BookingStatusEnum.Cancelled;
        existingBooking.UpdateAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Result.Success();
    }
    public async Task<Result> AdminCancelHotelBookingAsync(int hotelId, int bookingId)
    {
        // Get userId 
        var userId = _contextAccessor
            .HttpContext?
            .User?
            .FindFirst(type: JwtRegisteredClaimNames.Sub)?
            .Value;

        // Check if user is HotelAdmin user
        var isHotelAdminUser = await _context.HotelAdmins
            .AnyAsync(q => q.UserId == userId && q.HotelId == hotelId);

        if (!isHotelAdminUser)
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.Forbid, Description: $"You're not the Admin of the selected hotel."));
        }

        // Get booking
        var booking = await _context.Bookings
            .Include(q => q.Hotel)
            .FirstOrDefaultAsync(q =>
            q.Id == bookingId
            && q.HotelId == hotelId
            );

        if (booking is null)
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.NotFound, Description: $"Booking {bookingId} was not found. "));
        }
        if (booking.Status == BookingStatusEnum.Cancelled)
        {
            return Result.Failure(new Error(Code: ErrorCodes.Conflict, Description: "Cancelled bookings cannot be modified."));
        }

        // Cancel booking and save
        booking.Status = BookingStatusEnum.Cancelled;
        booking.UpdateAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Success();
    }
    public async Task<Result> AdminConfirmHotelBookingAsync(int hotelId, int bookingId)
    {
        // Get user
        var userId = _contextAccessor
            .HttpContext?
            .User?
            .FindFirst(type: JwtRegisteredClaimNames.Sub)?
            .Value;

        // Check if user is HotelAdmin user
        var isHotelAdminUser = await _context.HotelAdmins
            .AnyAsync(q =>
            q.UserId == userId
            && q.HotelId == hotelId
            );

        if (!isHotelAdminUser)
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.Forbid, Description: $"You're not the Admin of the selected hotel."));
        }

        var booking = await _context.Bookings
           .Include(q => q.Hotel)
           .FirstOrDefaultAsync(q =>
           q.Id == bookingId
           && q.HotelId == hotelId
         );

        if (booking is null)
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.NotFound, Description: $"Booking {bookingId} was not found. "));
        }
        if (booking.Status == BookingStatusEnum.Cancelled)
        {
            return Result.Failure(new Error(Code: ErrorCodes.Conflict, Description: "Cancelled bookings cannot be modified."));
        }

        // Confirm booking and save
        booking.Status = BookingStatusEnum.Confirmed;
        booking.UpdateAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Result.Success();
    }
    #endregion
}

