using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListingAPI.Constants;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Country;
using HotelListingAPI.Results;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Services;

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
    public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
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

        var countries = await _context.Countries
            .ProjectTo<GetCountriesDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<IEnumerable<GetCountriesDto>>.Success(countries);
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
            if (await CountryExistsAsync(name: country.Name))
            {
                return Result<GetCountryDto>
                    .Failure(new Error(Code: ErrorCodes.Conflict, Description: $"Country with name: {country.Name} already exists."));
            }

            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();

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

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await _context.Countries.AnyAsync(e => e.Id == id);
    }
    public async Task<bool> CountryExistsAsync(string name)
    {
        return await _context.Countries
           .AnyAsync(e => e.Name.ToLower().Trim() == name.ToLower().Trim());
    }
    #endregion
}
