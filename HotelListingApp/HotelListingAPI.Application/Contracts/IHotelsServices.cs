using HotelListingAPI.Application.DTOs.Hotel;
using HotelListingAPI.Common.Results;
using HotelListingAPI.Common.Models.Paging;
using HotelListingAPI.Common.Models.Filtering;

namespace HotelListingAPI.Application.Contracts;

public interface IHotelsServices
{
    #region Before Result Pattern
    #region Before Result Pattern
    //Task<IEnumerable<GetHotelDto>> GetHotelsAsync();
    //Task<GetHotelDto?> GetHotelAsync(int id);
    //Task<GetHotelDto?> CreateHotelAsync(CreateHotelDto hotelDto);
    //Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    //Task DeleteHotelAsync(int id);
    #endregion
    #endregion
    Task<Result<PagedResult<GetHotelDto>>> GetHotelsAsync(
        PaginationParameters paginationParameters,
        HotelFilterParameters filters);
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);

    Task<bool> HotelExistsAsync(int id);
    Task<bool> HotelExistsAsync(string name);
}
