namespace HotelListingAPI.Common.Models.Filtering;

public abstract class BasedFilterParameters
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;

}
