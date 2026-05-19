// Ignore Spelling: Dto

using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;

using HotelListingAPI.Application.DTOs.Country;
using HotelListingAPI.Application.DTOs.Hotel;
using HotelListingAPI.Common.Constants;
using HotelListingAPI.Common.Models.Extensions;
using HotelListingAPI.Common.Models.Filtering;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Common.Results;
using HotelListingAPI.Domain;

namespace HotelListingAPI.Application.Services;

public class CountriesService : ICountriesServices
{
    #region Fields
    private readonly HotelListingsDbContext _context;
    private readonly IMapper _mapper;
    #endregion

    #region Constructor
    public CountriesService(
        HotelListingsDbContext context,
        IMapper mapper
        )
    {
        _context = context;
        _mapper = mapper;
    }
    #endregion

    #region Methods
    #region IEnumerable<> method
    //public async Task<IEnumerable<GetCountriesDto>> GetCountriesAsync()
    //{       
    //    var countries = await _context.Countries
    //        .ProjectTo<GetCountriesDto>(_mapper.ConfigurationProvider)
    //        .ToListAsync();

    //    return countries;
    //}
    #endregion
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilterParameters filters)
    {
        var query = _context.Countries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters?.Search))
        {
            #region Filters

            // Filter by Country Name or ShortName colums
            var term = filters.Search.Trim();
            query = query
                .Where(q =>
                EF.Functions.Like(q.Name, $"%{term}%")
                ||
                EF.Functions.Like(q.ShortName, $"%{term}%"));
        }
        // Sort by country name ASC/DESC
        query = filters.SortBy?.ToLower() switch
        {
            "countryname" =>
            filters.SortDescending
            ?
            query.OrderByDescending(q => q.Name)
            :
            query.OrderBy(q => q.Name),

            // Default - sort by country name - ASC
            _ =>
           query.OrderBy(q => q.Name)
        };
            #endregion
        var countries = await query
            .AsNoTracking()
            .ProjectTo<GetCountriesDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
        #region Manual Mapping
        //var countries = await _context.Countries
        //    .Select(q => new GetCountriesDto(
        //        Id: q.Id,
        //        Name: q.Name,
        //        ShortName: q.ShortName
        //        ))
        //    .ToListAsync();
        #endregion
        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
    }

    public async Task<Result<GetCountryHotelsDto>> GetCountriesHotelsAsync(
        int countryId,
        PaginationParameters paginationParameters,
        CountryFilterParameters filters)
    {
        #region Manual Mapping
        //var countries = await _context.Countries
        //    .Select(q => new GetCountriesDto(
        //        Id: q.Id,
        //        Name: q.Name,
        //        ShortName: q.ShortName
        //        ))
        //    .ToListAsync();
        #endregion

        // Check if country exists
        if (!await CountryExistsAsync(countryId))
        {
            return Result<GetCountryHotelsDto>
                .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Country {countryId} was not found."));
        }
        // Filter and retrieve only CountryName
        var countryName = await _context.Countries
            .Where(q => q.Id == countryId)
            .Select(q => q.Name)
            .SingleAsync();

        // Filter hotels based on CountryId
        var hotelsQuery = _context.Hotels
            .Where(q => q.CountryId == countryId)
            .AsQueryable();

        // Filter by hotel name
        if (!string.IsNullOrEmpty(filters.Search))
        {
            var term = filters.Search.Trim();
            hotelsQuery = hotelsQuery.Where(q => EF.Functions.Like(q.Name, $"%{term}%"));
        }

        // Sort by
        hotelsQuery = (filters.SortBy?.ToLower().Trim().ToLowerInvariant()) switch
        {
            // By HotelName
            "name" =>
            filters.SortDescending
            ?
            hotelsQuery.OrderByDescending(q => q.Name)
            :
            hotelsQuery.OrderBy(q => q.Name),

            // By Rating
            "rating" =>
            filters.SortDescending
            ?
            hotelsQuery.OrderByDescending(q => q.Rating)
            :
            hotelsQuery.OrderBy(q => q.Rating),


            _ => hotelsQuery.OrderBy(q => q.Name)
        };

        var pagedHotels = await hotelsQuery
            .ProjectTo<GetHotelDto>(_mapper.ConfigurationProvider)
            .ToPagedResultAsync(paginationParameters);

        var result = new GetCountryHotelsDto
        {
            CountryId = countryId,
            Name = countryName,
            Hotels = pagedHotels
        };

        return Result<GetCountryHotelsDto>.Success(result);
    }
    #region Before Result pattern
    //public async Task<GetCountryDto?> GetCountryAsync(int id)
    //{
    //    #region Manual Mapping
    //    //  return selected country including all hotels
    //    var countryDto = await _context.Countries
    //        .Where(q => q.Id == id)
    //        .Select(q => new GetCountryDto(
    //            Id: q.Id,
    //            Name: q.Name,
    //            ShortName: q.ShortName,
    //            Hotels: q.Hotels
    //            .Select(q => new GetHotelsDto(
    //                Id: q.Id,
    //                Name: q.Name,
    //                Address: q.Address,
    //                Rating: q.Rating,
    //                CountryId: q.CountryId)).ToList()
    //            ))
    //        .FirstOrDefaultAsync();
    //    #endregion       

    //    if (countryDto == null)
    //    {
    //        return null;
    //    }

    //    return countryDto;
    //}
    #endregion
    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        //  return selected country including all hotels
        var countrDto = await _context.Countries
            .Where(c => c.Id == id)
            .ProjectTo<GetCountryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (!await CountryExistsAsync(id))
        {
            return Result<GetCountryDto>
                .NotFound(errors: new Error(Code: ErrorCodes.NotFound, Description: $"Country with Id: {id} not found."));
        }

        return countrDto is null
             ?
             Result<GetCountryDto>.NotFound()
             :
             Result<GetCountryDto>.Success(countrDto);
    }

    #region Before Result Pattern
    //public async Task<GetCountryDto> CreateCountry(CreateCountryDto createCountryDto)
    //{   
    #region Manual Mapping
    //var country = new Country
    //{
    //    Name = countryDto.Name,
    //    ShortName = countryDto.ShortName
    //};
    #endregion
    //    var country = _mapper.Map<Country>(createCountryDto);

    //    // Check for id duplicate
    //    await CountryExistsAsync(id: country.Id);

    //    // Check for name duplicate
    //    await CountryExistsAsync(name: country.Name);

    //    await _context.Countries.AddAsync(country);
    //    await _context.SaveChangesAsync();
    #region Manual Mapping
    //var resultDto = new GetCountryDto(
    //    Id: country.Id,
    //    Name: country.Name,
    //    ShortName: country.ShortName,
    //    Hotels: []
    //    );
    #endregion
    //    var resultDto = _mapper.Map<GetCountryDto>(source: country);

    //    return resultDto;
    //}
    #endregion
    public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto createCountryDto)
    {
        try
        {
            var country = _mapper.Map<Country>(createCountryDto);

            // Check for name duplicate
            if (await CountryExistsAsync(name: country.Name, countryId: country.Id))
            {
                return Result<GetCountryDto>
                    .Failure(new Error(Code: ErrorCodes.Conflict, Description: $"Country with name: {country.Name} already exists."));
            }

            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();

            // Map to Dto
            var resultDto = _mapper.Map<GetCountryDto>(source: country);

            return Result<GetCountryDto>.Success(resultDto);
        }
        catch (Exception)
        {
            return Result<GetCountryDto>.Failure();
        }
    }

    #region Before Result Pattern
    //public async Task UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    //{
    //    // Get country
    //    var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

    //    if (country == null)
    //    {
    //        throw new KeyNotFoundException(message: "Country not found.");
    //    }
    //    #region Manual Mapping
    ////    country.Name = updateDto.Name;
    ////    country.ShortName = updateDto.ShortName;
    //    #endregion

    //    // update country records and save
    //    _mapper.Map<UpdateCountryDto>(source: country);
    //    _context.Update(country);
    //    await _context.SaveChangesAsync();
    //}
    #endregion
    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    {

        if (!await CountryExistsAsync(id: id))
        {
            return Result
                .BadRequest(new Error(Code: ErrorCodes.Validation, Description: $"Invalid route Id: {id}"));
        }
        if (id != updateDto.Id)
        {
            return Result
                .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Country with Id: {updateDto.Id} not found."));
        }

        // Get country
        var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);
        if (country == null)
        {
            return Result
                .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Country with Id: {id} not found."));
        }
        // update country records and save
        _mapper.Map(updateDto, country);

        if (await CountryExistsAsync(name: country.Name, countryId: country.Id))
        {
            return Result
                .Failure(new Error(Code: ErrorCodes.Conflict, Description: $"Country with name: {country.Name} already exists."));
        }

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> PatchCountryAsync
        (int id, JsonPatchDocument<UpdateCountryDto> patchDoc)
    {
        // Get country
        var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);
        if (country == null)
        {
            return Result
                .NotFound(new Error(Code: ErrorCodes.NotFound, Description: $"Country with Id: {id} not found."));
        }
        // Map country to Dto
        var countryDto = _mapper.Map<UpdateCountryDto>(country);

        // Apply changes to countryDto
        patchDoc.ApplyTo(countryDto);

        if (countryDto.Id != id)
        {
            return Result
                .BadRequest(new Error(Code: ErrorCodes.Validation, Description: $"Cannot modify Id field."));
        }

        // Map to Domain model and save
        _mapper.Map(countryDto, country);

        // Check for CountryName duplicates
        if (await CountryExistsAsync(country.Name, country.Id))
        {
            return Result
              .BadRequest(new Error(Code: ErrorCodes.Conflict, Description: $"Country Name already exists."));
        }

        await _context.SaveChangesAsync();

        return Result.Success();
    }

    #region Delete - Before Result Pattern
    //public async Task DeleteCountryAsync(int id)
    //{
    //    var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

    //    if (country is null)
    //    {
    //        throw new KeyNotFoundException(message: $"{nameof(Country)} cannot be null");
    //    }

    //    _context.Countries.Remove(country);
    //    await _context.SaveChangesAsync();
    //}
    #endregion
    public async Task<Result> DeleteCountryAsync(int id)
    {
        if (!await CountryExistsAsync(id))
        {
            return Result
                .NotFound(new Error(Code: ErrorCodes.NotFound, $"Country with Id: {id} was not found."));
        }

        try
        {
            var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure();
        }
    }

    #region Validation Methods

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await _context.Countries.AnyAsync(e => e.Id == id);
    }
    public async Task<bool> CountryExistsAsync(string name, int countryId)
    {
        return await _context.Countries
           .AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim() && e.Id != countryId);
    }
    #endregion
    #endregion
}
