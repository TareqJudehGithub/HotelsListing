using HotelListingAPI.Application.DTOs.Hotel;

namespace HotelListingAPI.Application.DTOs.Country
{
    public class GetCountryDto
    {
        public int CountryId { get; set; }
        public required string Name { get; set; }
        public required string ShortName { get; set; }
        public List<GetHotelsDto> Hotels { get; set; } = [];
    }
}
