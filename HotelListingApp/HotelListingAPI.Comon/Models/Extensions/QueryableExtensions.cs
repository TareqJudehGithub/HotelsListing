// Ignore Spelling: Queryable

using HotelListingAPI.Common.Models.Paging;
using Microsoft.EntityFrameworkCore;

namespace HotelListingAPI.Common.Models.Extensions;

public static class QueryableExtensions
/* 
 adds an async pagination extension to any IQueryable<T> so callers can get paged data plus pagination metadata.
 */
{
    #region Extension methods
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>
        // Target data type:
        (this IQueryable<T> source,
            // value:
            PaginationParameters paginationParameters)
    {
        // EF Core helpers  (CountAsync, ToListAsync) 
        // Run a COUNT(*) query.
        var totalCount = await source.CountAsync();

        // Fetch the page items with Skip(page-1)*pageSize).Take(pageSize).ToListAsync() 
        var items = await source
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync();

        // Compute total pages: total items / page size
        var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);

        // New PaginationMetadata instance 
        var metaData = new PaginationMetadata
        {
            CurrentPage = paginationParameters.PageNumber,
            PageSize = paginationParameters.PageSize,
            TotalCounts = totalCount,
            TotalPages = totalPages,
            HasNext = paginationParameters.PageNumber < totalPages,
            HasPrevious = paginationParameters.PageNumber > 1
        };

        // 5.	Return a PagedResult<T> with items and metaData
        return new PagedResult<T>
        {
            Data = items,
            Metadata = metaData
        };
    }
    #endregion
}
