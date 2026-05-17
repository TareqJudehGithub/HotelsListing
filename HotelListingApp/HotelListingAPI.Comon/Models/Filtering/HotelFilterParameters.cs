namespace HotelListingAPI.Common.Models.Filtering;

public class HotelFilterParameters : BasedFilterParameters
{
    // Filter items by:
    public string? HotelName { get; set; }
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public double? MinRating { get; set; }
    public double? MaxRating { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Location { get; set; }
}
