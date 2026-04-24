using HotelListingAPI.DTOs.Hotel;

namespace HotelListingAPI.DTOs.Country
{
    public record GetCountryDto
    (
        int Id,
        string Name,
        string ShortName,
        List<GetHotelsDto> Hotels
        );
}
