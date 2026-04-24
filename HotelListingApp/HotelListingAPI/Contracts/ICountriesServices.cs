using HotelListingAPI.Data;
using HotelListingAPI.DTOs.Country;

namespace HotelListingAPI.Contracts;

public interface ICountriesServices
{
    Task<IEnumerable<GetCountriesDto>> GetCountriesAsync();
    Task<GetCountryDto?> GetCountryAsync(int id);
    Task<GetCountryDto> CreateCountry(CreateCountryDto countryDto);
    Task UpdateCountryAsync(int id, UpdateCountryDto updateDto);
    Task DeleteCountryAsync(int Id);
    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
}
