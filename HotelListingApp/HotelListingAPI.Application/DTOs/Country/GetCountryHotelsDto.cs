using HotelListingAPI.Application.DTOs.Hotel;
using HotelListingAPI.Common.Models.Paging;

namespace HotelListingAPI.Application.DTOs.Country
{
    public class GetCountryHotelsDto
    {
        public int CountryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public PagedResult<GetHotelDto> Hotels { get; set; } = new();
    }
}
