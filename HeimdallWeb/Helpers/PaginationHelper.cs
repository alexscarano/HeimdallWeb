using HeimdallWeb.Models.Map;
using Microsoft.EntityFrameworkCore;

namespace HeimdallWeb.Helpers
{
    public static class PaginationHelper
    {
        public async static Task<PaginatedResult<T>> PaginatedResultWrapper<T>(
        IQueryable<T> query,
        int page,
        int pageSize
        ) where T : class
        {
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
