namespace HotelListingAPI.DTOs.Country
{
    public class GetCountriesDto
    {
        public int CountryId { get; set; }
        public required string Name { get; set; }
        public required string ShortName { get; set; }
    }

}
