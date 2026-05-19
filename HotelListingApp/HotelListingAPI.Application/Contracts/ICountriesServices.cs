using HotelListingAPI.Application.DTOs.Country;
using HotelListingAPI.Common.Models.Filtering;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Common.Results;
using Microsoft.AspNetCore.JsonPatch;

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

    Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync(CountryFilterParameters filters);
    Task<Result<GetCountryHotelsDto>> GetCountriesHotelsAsync(
        int countryId,
        PaginationParameters paginationParameters,
        CountryFilterParameters filters);
    Task<Result<GetCountryDto>> GetCountryAsync(int id);
    Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto);
    Task<Result> UpdateCountryAsync(int id, UpdateCountryDto updateDto);
    Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDoc);
    Task<Result> DeleteCountryAsync(int Id);

    Task<bool> CountryExistsAsync(int id);
    Task<bool> CountryExistsAsync(string name, int CountryId);
}
