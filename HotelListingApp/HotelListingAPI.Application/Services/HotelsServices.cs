// Ignore Spelling: Dto

using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListingAPI.Domain;
using HotelListingAPI.Common.Constants;
using HotelListingAPI.Common.Results;
using HotelListingAPI.Application.DTOs.Hotel;
using HotelListingAPI.Application.Contracts;
using HotelListingAPI.Common.Models.Extensions;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Common.Models.Filtering;

namespace HotelListingAPI.Application.Services
{
    public class HotelsServices : IHotelsServices
    {
        #region Fields
        private readonly HotelListingsDbContext _dbContext;
        private readonly IMapper _mapper;
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

        }
        #endregion
        #region Methods (Implementations)

        public async Task<Result<PagedResult<GetHotelDto>>> GetHotelsAsync(
            PaginationParameters paginationParameters,
            HotelFilterParameters filters)
        {
            #region Filters
            // Convert Hotels as queryables
            var query = _dbContext.Hotels.AsQueryable();

            // Filter By hotel name
            if (!string.IsNullOrWhiteSpace(filters.HotelName))
            {
                query = query
                    .Where(q => q.Name.Contains(filters.HotelName));
            }
            // Filter by CountryId
            if (filters.CountryId.HasValue)
            {
                query = query.Where(q => q.CountryId == filters.CountryId);
            }
            // Filter By country name
            if (!string.IsNullOrWhiteSpace(filters.CountryName))
            {
                query = query
                    .Where(q => q.Country.Name! == filters.CountryName);
            }
            // Filter by Min. Rating
            if (filters.MinRating.HasValue)
            {
                query = query.Where(q => q.Rating >= filters.MinRating);
            }
            // Filter by Max. Rating
            if (filters.MaxRating.HasValue)
            {
                query = query.Where(q => q.Rating <= filters.MaxRating);
            }
            // Filter by Min. Price
            if (filters.MinPrice.HasValue)
            {
                query = query.Where(q => q.PerNightRate >= filters.MinPrice);
            }
            // Filter by Max. Price
            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(q => q.PerNightRate <= filters.MaxPrice);
            }
            // Filter by Max. Location
            if (!string.IsNullOrWhiteSpace(filters.Location))
            {
                var location = filters.Location.Trim();
                query = query
                    .Where(q => EF.Functions.Like(q.Address, $"%{location}%"));
            }
            // Generic search param
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.Trim();
                query = query
                    .Where(q =>
                    EF.Functions.Like(q.Name, $"%{search}%")
                    ||
                    EF.Functions.Like(q.Address, $"%{search}%"));
            }

            query = filters.SortBy?.ToLower() switch
            {
                "name" => filters.SortDescending ?
                    query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
                "rating" => filters.SortDescending ?
                    query.OrderByDescending(h => h.Rating) : query.OrderBy(h => h.Rating),
                "price" => filters.SortDescending ?
                    query.OrderByDescending(h => h.PerNightRate) : query.OrderBy(h => h.PerNightRate),
                _ => query.OrderBy(h => h.Name)
            };

            #endregion

            // Auto Mapper 
            var hotelsDto = await query
                    .Include(q => q.Country)
                    .ProjectTo<GetHotelDto>(_mapper.ConfigurationProvider)
                    .ToPagedResultAsync(paginationParameters);

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

            if (hotelsDto.Data.Count() == 0)
            {
                return Result<PagedResult<GetHotelDto>>
                    .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel list is empty."));
            }

            return Result<PagedResult<GetHotelDto>>.Success(value: hotelsDto);

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
            if (!await HotelExistsAsync(id))
            {
                return Result.NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Hotel not found with Id: {id}"));
            }
            // Check for country Id


            if (!await _dbContext.Countries.AnyAsync(q => q.Id == hotel.CountryId))
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

            if (!await _dbContext.Countries.AnyAsync(q => q.Id == hotel.CountryId))
            {
                return Result
                   .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Country with Id: {hotel.CountryId} was not found."));
            }

            if (await HotelExistsAsync(name: hotel.Name))
            {
                return Result
                    .Failure(new Error(Code: ErrorCodes.Conflict, Description: $"Hotel with name: {hotel.Name} already exists."));
            }
            // Check if Hotel do exists
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

        #region Validations

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

        #endregion
    }
}
