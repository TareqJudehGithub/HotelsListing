using HotelListingAPI.Application.DTOs.Country;
using HotelListingAPI.Application.DTOs.Hotel;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Common.Results;

public interface ICountriesServices
{
    #region Code before Results pattern
    //Task<IEnumerable<GetCountriesDto>> GetCountriesAsync();
    //Task<GetCountryDto?> GetCountryAsync(int id);
    //Task<GetCountryDto> CreateCountryAsync(CreateCountryDto countryDto);
    //Task UpdateCountryAsync(int id, UpdateCountryDto updateDto);
    //Task DeleteCountryAsync(int Id);

    //Task<bool> CountryExistsAsync(int id);
    //Task<bool> CountryExistsAsync(string name);
    #endregion

    Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync();
    Task<Result<PagedResult<GetHotelDto>>> GetCountriesHotelsAsync(
        int countryId,
        PaginationParameters paginationParameters);

    Task<Result<GetCountryDto>> GetCountryAsync(int id);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto);
    Task<Result> DeleteCountryAsync(int Id);

    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name);
}
