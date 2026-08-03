using Microsoft.EntityFrameworkCore;

namespace Schedulas.Application.Common.Models;

/// <summary>
/// Standard shape for every list Query response, feeding directly into the
/// API envelope's `data: { items, totalCount, pageNumber, pageSize }`
/// (Constitution §11 pagination standard).
/// </summary>
public sealed class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var totalCount = await source.CountAsync(ct);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}
