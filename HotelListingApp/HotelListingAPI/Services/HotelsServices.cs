using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Hotel;
using HotelListingAPI.Results;
using HotelListingAPI.Constants;

namespace HotelListingAPI.Services
{
    public class HotelsServices : IHotelsServices
    {
        #region Fields
        private readonly HotelListingsDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ICountriesServices _countriesServices;
        #endregion
        #region Constructor
        public HotelsServices(
            HotelListingsDbContext dbContext,
            IMapper mapper,
            ICountriesServices countriesServices
            )
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _countriesServices = countriesServices;
        }
        #endregion
        #region Methods (Implementations)

        public async Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync()
        {
            #region Before Result pattern
            //     public async Task<IEnumerable<GetHotelDto>> GetHotelsAsync()
            //{
            //    // Auto Mapper
            //    var hotelsDto = await _dbContext.Hotels
            //        .Include(q => q.Country)
            //        .ProjectTo<GetHotelDto>(_mapper.ConfigurationProvider)
            //        .ToListAsync();

            //    #region Manual Mapping
            //    // Manual mapping
            //    //var hotelsDto = await _dbContext.Hotels
            //    //    .Include(q => q.Country)
            //    //    .Select(q => new GetHotelDto(
            //    //        Id: q.Id,
            //    //        Name: q.Name,
            //    //        Address: q.Address,
            //    //        Rating: q.Rating,
            //    //        CountryId: q.CountryId,
            //    //        Country: q.Country!.CountryName
            //    //        ))
            //    //    .ToListAsync();
            //    #endregion 

            //    return hotelsDto;
            //}
            #endregion

            // Auto Mapper 
            var hotelsDto = await _dbContext.Hotels
                    .Include(q => q.Country)
                    .ProjectTo<GetHotelDto>(_mapper.ConfigurationProvider)
                    .ToListAsync();

            #region Manual Mapping
            // Manual mapping
            //var hotelsDto = await _dbContext.Hotels
            //    .Include(q => q.Country)
            //    .Select(q => new GetHotelDto(
            //        Id: q.Id,
            //        Name: q.Name,
            //        Address: q.Address,
            //        Rating: q.Rating,
            //        CountryId: q.CountryId,
            //        Country: q.Country!.CountryName
            //        ))
            //    .ToListAsync();
            #endregion


            if (hotelsDto.Count == 0)
            {
                return Result<IEnumerable<GetHotelDto>>
                    .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel list is empty."));
            }

            return Result<IEnumerable<GetHotelDto>>.Success(value: hotelsDto);
        }
        public async Task<Result<GetHotelDto>> GetHotelAsync(int id)
        {
            // AutoMapper
            var hotelDto = await _dbContext.Hotels
                .Where(q => q.Id == id)
                .Include(q => q.Country)
                .ProjectTo<GetHotelDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            #region Manual Mapping
            // Manual Mapping
            //var hotelDto = await _dbContext.Hotels
            //     .Where(q => q.Id == id)
            //     .Include(q => q.Country)
            //     .Select(q => new GetHotelDto(
            //         Id: q.Id,
            //         Name: q.Name,
            //         Address: q.Address,
            //         Rating: q.Rating,
            //         CountryId: q.CountryId,
            //         CountryName: q.Country!.Name))
            //     .FirstOrDefaultAsync();

            //if (hotelDto is null)
            //{
            //    throw new KeyNotFoundException(message: "Hotel not found Error.");
            //}
            #endregion

            if (hotelDto is null)
            {
                return Result<GetHotelDto>.NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel with Id: {id} was not found."));
            }
            return Result<GetHotelDto>.Success(value: hotelDto);

        }
        public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto createHotelDto)
        {
            #region  Before Result pattern
            //var hotel = _mapper.Map<Hotel>(source: createHotelDto);

            //// Check of Id duplication
            //if (await HotelExistsAsync(id: hotel.Id))
            //{
            //    throw new InvalidOperationException(message: "Hotel Id already exists");
            //}
            //// Check for name duplication
            //if (await HotelExistsAsync(name: hotel.Name))
            //{
            //    throw new InvalidOperationException(message: $"Hotel Name {hotel.Name} already exists");
            //}

            //// Add and Save into the DB
            //await _dbContext.Hotels.AddAsync(hotel);
            //await _dbContext.SaveChangesAsync();

            //// Return newly created hotel along with country name
            //var hotelWithCountry = await _dbContext.Hotels
            //.Include(h => h.Country)
            //.FirstOrDefaultAsync(h => h.Id == hotel.Id);

            //// Check for null
            //if (hotelWithCountry == null) return null;

            // Convert to Dto and return

            #region Manual Mapping
            //var hotelDto = new GetHotelDto(
            //    hotelWithCountry.Id,
            //    hotelWithCountry.Name,
            //    hotelWithCountry.Address,
            //    hotelWithCountry.Rating,
            //    hotelWithCountry.CountryId,
            //    hotelWithCountry.Country!.Name
            //);
            #endregion

            //var hotelDto = _mapper.Map<GetHotelDto>(source: hotel);

            //return hotelDto;

            #endregion

            #region Manual Mapping
            // Create new hotel model instance
            //var hotel = new Hotel
            //{
            //    Name = createHotelDto.Name,
            //    Address = createHotelDto.Address,
            //    Rating = createHotelDto.Rating,
            //    CountryId = createHotelDto.CountryId
            //};
            #endregion

            var hotel = _mapper.Map<Hotel>(source: createHotelDto);

            // Check if Country exists
            if (!await HotelExistsAsync(id: hotel.CountryId))
            {
                return Result<GetHotelDto>
                    .Failure(new Error(Code: ErrorCodes.BadRequest, Description: $"Country with Id: {hotel.CountryId} was not found."));
            }
            // Check for name duplication
            if (await HotelExistsAsync(name: hotel.Name))
            {
                return Result<GetHotelDto>
                    .Failure(new Error(Code: ErrorCodes.Conflict, Description: $"Hotel Name {hotel.Name} already exists"));
            }
            // Add and Save into the DB
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();

            // Return newly created hotel along with country name
            var hotelWithCountry = await _dbContext.Hotels
            .Include(h => h.Country)
            .FirstOrDefaultAsync(h => h.Id == hotel.Id);

            // Check for null
            if (hotelWithCountry is null)
            {
                return Result<GetHotelDto>
                     .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel with Id: {hotelWithCountry.Id} was not found."));
            }


            #region Manual Mapping
            //var hotelDto = new GetHotelDto(
            //    hotelWithCountry.Id,
            //    hotelWithCountry.Name,
            //    hotelWithCountry.Address,
            //    hotelWithCountry.Rating,
            //    hotelWithCountry.CountryId,
            //    hotelWithCountry.Country!.Name
            //);
            #endregion

            // Convert to Dto and return
            var hotelDto = _mapper.Map<GetHotelDto>(source: hotel);

            return Result<GetHotelDto>.Success(hotelDto);
        }

        public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto updateHotelDto)
        {
            #region Before Result pattern
            //// Get Hotel
            //var hotel = _dbContext.Hotels.FirstOrDefault(h => h.Id == id);

            //// Check for null
            //if (hotel == null)
            //{
            //    throw new KeyNotFoundException(message: $"Hotel not found error.");
            //}

            //#region Manual Mapping
            //// Update hotel records
            ////hotel.Name = hotelDto.Name;
            ////hotel.Address = hotelDto.Address;
            ////hotel.Rating = hotelDto.Rating;
            ////hotel.CountryId = hotelDto.CountryId;
            //#endregion

            //// AutoMapper
            //_mapper.Map<UpdateHotelDto>(source: hotel);

            //// Check if hotel name exists
            //if (await HotelExistsAsync(name: hotel.Name) && hotel.Id != id)
            //{
            //    throw new InvalidOperationException(message: $"Hotel Name {hotel.Name} already exists");
            //}
            //// Save
            //_dbContext.Update(hotel);
            //await _dbContext.SaveChangesAsync();
            #endregion  

            // Check for Id 
            if (!await HotelExistsAsync(id: id))
            {
                return Result
                    .BadRequest(new Error(Code: ErrorCodes.BadRequest, Description: $"Hotel with Id: {id} was not found. -  route Id "));
            }

            // Get Hotel
            var hotel = await _dbContext.Hotels.FirstOrDefaultAsync(h => h.Id == id);

            // Check for null
            if (hotel is null)
            {
                return Result.NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel not found with Id: {id}"));
            }
            // Check Hotel Id
            if (id != hotel.Id)
            {
                return Result.NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel not found with Id: {id}"));
            }
            // Check for country Id
            if (!await HotelExistsAsync(id: hotel.CountryId))
            {
                return Result
                    .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Country with Id: {id} was not found."));
            }
            #region Manual Mapping
            // Update hotel records
            //hotel.Name = hotelDto.Name;
            //hotel.Address = hotelDto.Address;
            //hotel.Rating = hotelDto.Rating;
            //hotel.CountryId = hotelDto.CountryId;
            #endregion

            // Check if hotel name exists
            // AutoMapper
            _mapper.Map(updateHotelDto, hotel);

            if (await HotelExistsAsync(name: hotel.Name))
            {
                return Result
                    .Failure(new Error(Code: ErrorCodes.Conflict, Description: $"Hotel with name: {hotel.Name} already exists."));
            }
            // Check if Country do exists
            if (!await HotelExistsAsync(id: hotel.CountryId))
            {
                return Result
                    .Failure(new Error(Code: ErrorCodes.BadRequest, Description: $"Country with Id: {hotel.CountryId} was not found."));
            }

            // Save
            _dbContext.Update(hotel);
            await _dbContext.SaveChangesAsync();

            return Result.Success();
        }
        public async Task<Result> DeleteHotelAsync(int id)
        {
            var hotel = await _dbContext.Hotels
                .Where(h => h.Id == id)
                .ExecuteDeleteAsync();

            if (hotel == 0)
            {
                return Result.NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel not found with Id: {id}"));
            }

            return Result.Success();

            #region Delete alternative
            // Or 
            //   var hotel = await _dbContext.Hotels
            //.Include(h => h.Country)
            //.FirstOrDefaultAsync(q => q.Id == id);

            //   if (hotel == null)
            //   {
            //       throw new KeyNotFoundException(message: $"Hotel with Id: {id} was not found!");
            //   }
            //   // Save changes to DB
            //   _dbContext.Hotels.Remove(hotel);
            //   await _dbContext.SaveChangesAsync();
            #endregion
        }

        // Check if hotel exists in DB
        public async Task<bool> HotelExistsAsync(int id)
        {
            return await _dbContext.Hotels.AnyAsync(e => e.Id == id);
        }
        public async Task<bool> HotelExistsAsync(string name)
        {
            return await _dbContext.Hotels.AnyAsync(e => e.Name == name);
        }
        #endregion
    }
}
