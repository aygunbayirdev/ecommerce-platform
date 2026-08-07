using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.BuildingBlocks.Application.Models;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

/// <summary>Raw paged rows, no product name/SKU — the Api layer enriches those via Catalog (composition-root pattern, avoids a Catalog.Application &lt;-&gt; Inventory.Application circular reference).</summary>
public sealed class GetAllStockItemsQueryHandler(IStockItemReadRepository stockItemReadRepository)
    : IQueryHandler<GetAllStockItemsQuery, PagedResult<StockItemDto>>
{
    public async Task<Result<PagedResult<StockItemDto>>> Handle(
        GetAllStockItemsQuery request, CancellationToken cancellationToken)
    {
        var page = await stockItemReadRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);

        return Result.Success(page);
    }
}
