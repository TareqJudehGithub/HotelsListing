using HotelListingAPI.DTOs.Hotel;

namespace HotelListingAPI.Contracts;

public interface IHotelsServices
{
    Task<IEnumerable<GetHotelDto>> GetHotelsAsync();
    Task<GetHotelDto?> GetHotelAsync(int id);
    Task<GetHotelDto?> CreateHotelAsync(CreateHotelDto hotelDto);
    Task UpdateHotelAsync(int id, UpdateHotelDto hotelDto);
    Task DeleteHotelAsync(int id);
    Task<bool> HotelExistsAsync(int id);
    Task<bool> HotelExistsAsync(string name);
}
