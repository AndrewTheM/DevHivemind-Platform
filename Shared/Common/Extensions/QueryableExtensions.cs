using BlogPlatform.Shared.Common.Filters;

namespace BlogPlatform.Shared.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> records, PaginationFilter filter)
    {
        if (filter.PageNumber == default && filter.PageSize == default)
            return records;

        return records.Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize);
    }
}
