namespace HotelListingAPI.Common.Models.Paging;

public class PaginationMetadata
{
    #region Properties
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCounts { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    #endregion
}
