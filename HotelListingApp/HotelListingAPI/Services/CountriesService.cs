using HotelListingAPI.Contracts;
using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Country;
using HotelListingAPI.DTOs.Hotel;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Services;

public class CountriesService : ICountriesServices
{
    #region Fields
    private readonly HotelListingsDbContext _context;
    #endregion

    #region Constructor
    public CountriesService(HotelListingsDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Methods
    public async Task<IEnumerable<GetCountriesDto>> GetCountriesAsync()
    {
        var countries = await _context.Countries
            .Select(q => new GetCountriesDto(
                Id: q.Id,
                Name: q.Name,
                ShortName: q.ShortName
                ))
            .ToListAsync();

        return countries;
    }
    public async Task<GetCountryDto?> GetCountryAsync(int id)
    {
        // return selected country including all hotels 
        var country = await _context.Countries
            .Where(q => q.Id == id)
            .Select(q => new GetCountryDto(
                Id: q.Id,
                Name: q.Name,
                ShortName: q.ShortName,
                Hotels: q.Hotels
                .Select(q => new GetHotelsDto(
                    Id: q.Id,
                    Name: q.Name,
                    Address: q.Address,
                    Rating: q.Rating,
                    CountryId: q.CountryId)).ToList()
                ))
            .FirstOrDefaultAsync();

        if (country == null)
        {
            return null;
        }

        return country;
    }

    public async Task<GetCountryDto> CreateCountry(CreateCountryDto countryDto)
    {
        var country = new Country
        {
            Name = countryDto.Name,
            ShortName = countryDto.ShortName
        };

        // Check for id duplicate
        await CountryExistsAsync(id: country.Id);

        // Check for name duplicate
        await CountryExistsAsync(name: country.Name);

        await _context.Countries.AddAsync(country);
        await _context.SaveChangesAsync();

        var resultDto = new GetCountryDto(
            Id: country.Id,
            Name: country.Name,
            ShortName: country.ShortName,
            Hotels: []
            );

        return resultDto;
    }
    public async Task UpdateCountryAsync(int id, UpdateCountryDto updateDto)
    {
        // Get country
        var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

        if (country == null)
        {
            throw new KeyNotFoundException(message: "Country not found.");
        }

        // update country records and save
        country.Name = updateDto.Name;
        country.ShortName = updateDto.ShortName;
        _context.Update(country);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteCountryAsync(int id)
    {
        var country = await _context.Countries.FirstOrDefaultAsync(q => q.Id == id);

        if (country is null)
        {
            throw new KeyNotFoundException(message: $"{nameof(Country)} cannot be null");
        }

        _context.Countries.Remove(country);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await _context.Countries.AnyAsync(e => e.Id == id);
    }
    public async Task<bool> CountryExistsAsync(string name)
    {
        return await _context.Countries.AnyAsync(e => e.Name == name);
    }

    #endregion
}
