using Microsoft.EntityFrameworkCore;
using AutoMapper;

using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Hotel;
using AutoMapper.QueryableExtensions;

namespace HotelListingAPI.Services
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
            IMapper mapper
            )
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        #endregion
        #region Methods (Implementations)
        public async Task<IEnumerable<GetHotelDto>> GetHotelsAsync()
        {
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

            return hotelsDto;
        }
        public async Task<GetHotelDto?> GetHotelAsync(int id)
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

            return hotelDto;
        }
        public async Task<GetHotelDto?> CreateHotelAsync(CreateHotelDto createHotelDto)
        {
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

            // Check of Id duplication
            if (await HotelExistsAsync(id: hotel.Id))
            {
                throw new InvalidOperationException(message: "Hotel Id already exists");
            }
            // Check for name duplication
            if (await HotelExistsAsync(name: hotel.Name))
            {
                throw new InvalidOperationException(message: $"Hotel Name {hotel.Name} already exists");
            }

            // Add and Save into the DB
            await _dbContext.Hotels.AddAsync(hotel);
            await _dbContext.SaveChangesAsync();

            // Return newly created hotel along with country name
            var hotelWithCountry = await _dbContext.Hotels
            .Include(h => h.Country)
            .FirstOrDefaultAsync(h => h.Id == hotel.Id);

            // Check for null
            if (hotelWithCountry == null) return null;

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

            var hotelDto = _mapper.Map<GetHotelDto>(source: hotel);

            return hotelDto;
        }
        public async Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
        {
            // Get Hotel
            var hotel = _dbContext.Hotels.FirstOrDefault(h => h.Id == id);

            // Check for null
            if (hotel == null)
            {
                throw new KeyNotFoundException(message: $"Hotel not found error.");
            }

            #region Manual Mapping
            // Update hotel records
            //hotel.Name = hotelDto.Name;
            //hotel.Address = hotelDto.Address;
            //hotel.Rating = hotelDto.Rating;
            //hotel.CountryId = hotelDto.CountryId;
            #endregion

            // AutoMapper
            _mapper.Map<UpdateHotelDto>(source: hotel);

            // Check if hotel name exists
            if (await HotelExistsAsync(name: hotel.Name) && hotel.Id != id)
            {
                throw new InvalidOperationException(message: $"Hotel Name {hotel.Name} already exists");
            }
            // Save
            _dbContext.Update(hotel);
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteHotelAsync(int id)
        {
            var hotel = await _dbContext.Hotels
                .Where(h => h.Id == id)
                .ExecuteDeleteAsync();
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
