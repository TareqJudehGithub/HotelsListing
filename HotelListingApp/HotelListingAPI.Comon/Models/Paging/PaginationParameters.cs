using System.ComponentModel.DataAnnotations;

namespace HotelListingAPI.Common.Models.Paging;

public class PaginationParameters
{
    #region Fields
    // Default number of records returning per page, in case PageSize was not set.
    private int _pageSize = 10;
    #endregion

    #region Properties
    // Max. number of records a page will return
    private const int MaxPageSize = 50;

    [Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(minimum: 1, maximum: MaxPageSize, ErrorMessage = "Page size must be between {1} and {2}")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    #endregion
}
