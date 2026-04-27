using HotelListingAPI.DTOs.Hotel;
using HotelListingAPI.Results;

namespace HotelListingAPI.Contracts;

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
    Task<Result<IEnumerable<GetHotelDto>>> GetHotelsAsync();
    Task<Result<GetHotelDto>> GetHotelAsync(int id);
    Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto);
    Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task<Result> DeleteHotelAsync(int id);

    Task<bool> HotelExistsAsync(int id);
    Task<bool> HotelExistsAsync(string name);
}
