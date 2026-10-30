using Microsoft.EntityFrameworkCore;
using Stores.Application.Common;

namespace Stores.Infrastructure.Persistence.Repositories;

internal static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, total);
    }
}
