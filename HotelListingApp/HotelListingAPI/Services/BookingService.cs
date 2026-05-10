using Microsoft.EntityFrameworkCore;

using HotelListingAPI.Constants;
using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.Data.Enums;
using HotelListingAPI.DTOs.Booking;
using HotelListingAPI.Results;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace HotelListingAPI.Services;

public class BookingService : IBookingService
{
    #region Fields
    private readonly HotelListingsDbContext _context;
    private readonly IUsersServices _usersServices;
    private readonly IMapper _mapper;
    #endregion

    #region Constructor
    public BookingService(
        HotelListingsDbContext context,
        IUsersServices usersServices,
        IMapper mapper)
    {
        _context = context;
        _usersServices = usersServices;
        _mapper = mapper;
    }

    #endregion

    #region Methods
    public async Task<Result<IEnumerable<GetBookingDto>>> UserGetHotelBookingsAsync(int hotelId)
    {
        // Get userId
        var userId = _usersServices.UserId();

        // Check if userId is not null
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IEnumerable<GetBookingDto>>
               .Failure(new Error(Code: ErrorCodes.Validation, Description: "User is required"));
        }

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
            .ProjectTo<GetBookingDto>(_mapper.ConfigurationProvider)
          .ToListAsync();

        #region Manual mapping
        //var bookings = await _context.Bookings
        //.Where(q => q.HotelId == hotelId)
        //.OrderBy(q => q.CheckIn)
        //.Select(q => new GetBookingDto
        //{
        //    Id = q.Id,
        //    HotelId = q.HotelId,
        //    HotelName = q.Hotel!.Name,
        //    CheckIn = q.CheckIn,
        //    CheckOut = q.CheckOut,
        //    Guests = q.Guests,
        //    TotalPrice = q.TotalPrice,
        //    Status = q.Status.ToString(),
        //    CreateAtUtc = q.CreateAtUtc,
        //    UpdateAtUtc = q.UpdateAtUtc,
        //})
        //.ToListAsync();

        #endregion
        return Result<IEnumerable<GetBookingDto>>.Success(bookings);
    }

    public async Task<Result<GetBookingDto>> CreateHotelBookingAsync(CreateBookingDto createBookingDto)
    {
        // Get userId
        var userId = _usersServices.UserId();

        // Check if userId is not null
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>
               .Failure(new Error(Code: ErrorCodes.Validation, Description: "User is required"));
        }

        #region Validations        
        var overlaps = await IsOverLap(createBookingDto.HotelId, userId, createBookingDto.CheckIn, createBookingDto.CheckOut);

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

        var nights = createBookingDto.CheckOut.DayNumber - createBookingDto.CheckIn.DayNumber;
        var totalPrice = hotel.PerNightRate * nights;

        // Map to Domain Model
        #region Manual Mapping
        //var booking = new Booking
        //{
        //    HotelId = createBookingDto.HotelId,
        //    UserId = userId,
        //    Guests = createBookingDto.Guests,
        //    CheckIn = createBookingDto.CheckIn,
        //    CheckOut = createBookingDto.CheckOut,
        //    Status = BookingStatusEnum.pending,
        //    TotalPrice = totalPrice,
        //};
        #endregion
        var booking = _mapper.Map<Booking>(source: createBookingDto);
        booking.UserId = userId;

        // Add the new booking to DB and save
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        // Map back to DTO Model
        var result = _mapper.Map<GetBookingDto>(source: booking);
        #region Manual Mapping
        //var createdBooking = new GetBookingDto
        //{
        //    Id = booking.Id,
        //    HotelId = hotel.Id,
        //    HotelName = hotel.Name,
        //    CheckIn = booking.CheckIn,
        //    CheckOut = booking.CheckOut,
        //    Guests = booking.Guests,
        //    TotalPrice = booking.TotalPrice,
        //    // Trevor's TotalPrice: TotalPrice = totalPrice
        //    Status = booking.Status.ToString(),
        //    CreateAtUtc = booking.CreateAtUtc,
        //    UpdateAtUtc = booking.UpdateAtUtc
        //};
        #endregion
        return Result<GetBookingDto>.Success(result);
    }
    public async Task<Result<GetBookingDto>> UpdateHotelBookingAsync(
        int hotelId,
        int bookingId,
        UpdateBookingDto updateBookingDto)
    {
        // Get userId
        var userId = _usersServices.UserId();

        // Check if userId is not null
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<GetBookingDto>
               .Failure(new Error(Code: ErrorCodes.Validation, Description: "User is required"));
        }
        #region Validations
        // Check for overlaps booking from the same user
        // Overlaps means that the new booking's check-in date is before an existing booking's check-out date
        var overlaps = await IsOverLap(hotelId, userId, updateBookingDto.CheckIn, updateBookingDto.CheckOut);

        if (overlaps)
        {
            return Result<GetBookingDto>
                .Failure(new Error(Code: ErrorCodes.Conflict, Description: "The selected dates overlap with an existing booking."));
        }
        #endregion

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


        // Update booking record
        _mapper.Map(source: updateBookingDto, destination: booking);
        #region Manual Mapping
        //booking.CheckIn = updateBookingDto.CheckIn;
        //booking.CheckOut = updateBookingDto.CheckOut;
        //booking.Guests = updateBookingDto.Guests;
        #endregion

        var perNight = booking.Hotel!.PerNightRate;
        var nights = updateBookingDto.CheckOut.DayNumber - updateBookingDto.CheckIn.DayNumber;
        booking.TotalPrice = perNight * nights;
        booking.UpdateAtUtc = DateTime.UtcNow;
        //      booking.UserId = userId;

        // Save changes
        await _context.SaveChangesAsync();

        // Map Back to Dto
        var updatedBooking = _mapper.Map<GetBookingDto>(source: booking);
        #region Manual Mapping
        //var updatedBooking = new GetBookingDto
        //{
        //    Id = booking.Id,
        //    HotelId = booking.HotelId,
        //    HotelName = booking.Hotel!.Name,
        //    CheckIn = booking.CheckIn,
        //    CheckOut = booking.CheckOut,
        //    Guests = booking.Guests,
        //    TotalPrice = booking.TotalPrice,
        //    Status = booking.Status.ToString(),
        //    CreateAtUtc = booking.CreateAtUtc,
        //    UpdateAtUtc = booking.UpdateAtUtc
        //};
        #endregion
        return Result<GetBookingDto>.Success(updatedBooking);
    }
    public async Task<Result<IEnumerable<GetBookingDto>>> AdminGetHotelBookingsAsync(int hotelId)
    {
        // Get userId
        var userId = _usersServices.UserId();

        // Check if userId is not null
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IEnumerable<GetBookingDto>>
               .Failure(new Error(Code: ErrorCodes.Validation, Description: "User is required"));
        }


        // Check if user is HotelAdmin
        var isHotelAdminUser = await _context.HotelAdmins
            .AnyAsync(q => q.UserId == userId && q.HotelId == hotelId);

        if (!isHotelAdminUser)
        {
            return Result<IEnumerable<GetBookingDto>>
                .Failure(new Error(Code: ErrorCodes.Forbid, Description: $"You're not the Admin of the selected hotel."));
        }

        // Check for hotel existence
        var hotel = await _context.Hotels
            .FirstOrDefaultAsync(q => q.Id == hotelId);

        if (hotel is null)
        {
            return Result<IEnumerable<GetBookingDto>>
                .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel with Id {hotelId} was not found"));
        }

        // return bookings

        var bookings = await _context.Bookings
            .Where(q => q.HotelId == hotelId)
            .OrderBy(q => q.CheckIn)
            .ProjectTo<GetBookingDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        #region Manual Mapping
        //var bookings = await _context.Bookings
        //    .Where(q => q.HotelId == hotelId)
        //    .OrderBy(q => q.CheckIn)
        //    .Select(q => new GetBookingDto
        //    {
        //        Id = q.Id,
        //        HotelId = q.HotelId,
        //        HotelName = q.Hotel!.Name,
        //        CheckIn = q.CheckIn,
        //        CheckOut = q.CheckOut,
        //        Guests = q.Guests,
        //        TotalPrice = q.TotalPrice,
        //        Status = q.Status.ToString(),
        //        CreateAtUtc = q.CreateAtUtc,
        //        UpdateAtUtc = q.UpdateAtUtc,
        //    })
        //    .ToListAsync();
        #endregion

        return Result<IEnumerable<GetBookingDto>>.Success(value: bookings);
    }
    public async Task<Result> CancelHotelBookingAsync(int hotelId, int bookingId)
    {
        // Get userId 
        var userId = _usersServices.UserId();

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
        var userId = _usersServices.UserId();

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
        // Get userId
        var userId = _usersServices.UserId();

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

    // Check for overlaps booking from the same user
    // Overlaps means that the new booking's check-in date is before an existing booking's check-out date
    private async Task<bool> IsOverLap(int hotelId, string userId, DateOnly checkIn, DateOnly checkOut)
    {
        return await _context.Bookings.AnyAsync(
             q => q.HotelId == hotelId
           && q.Status != BookingStatusEnum.Cancelled
           && q.CheckIn < q.CheckOut
           && q.CheckOut > q.CheckIn
           && q.UserId == userId
            );
    }
    #endregion
}


