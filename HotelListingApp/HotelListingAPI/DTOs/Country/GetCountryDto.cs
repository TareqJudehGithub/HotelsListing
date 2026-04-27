using HotelListingAPI.DTOs.Hotel;

namespace HotelListingAPI.DTOs.Country
{
    public class GetCountryDto
    {
        public int CountryId { get; set; }
        public required string Name { get; set; }
        public required string ShortName { get; set; }
        public List<GetHotelsDto> Hotels { get; set; } = [];
    }
}
