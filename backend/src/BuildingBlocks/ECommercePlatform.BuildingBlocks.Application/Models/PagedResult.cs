namespace ECommercePlatform.BuildingBlocks.Application.Models;

public sealed class PagedResult<TItem>
{
    public required IReadOnlyList<TItem> Items { get; init; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
