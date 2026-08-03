using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

/// <summary>
/// All-or-nothing: every item is validated against its StockItem before SaveChangesAsync is ever
/// called. If any item has insufficient stock, the handler returns immediately without saving —
/// the in-memory mutations already applied to other StockItems in this DbContext are simply
/// discarded along with it, so no explicit rollback/release step is needed.
/// </summary>
public sealed class ReserveStockCommandHandler(IStockItemWriteRepository stockItemWriteRepository)
    : ICommandHandler<ReserveStockCommand>
{
    public async Task<Result> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        var productVariantIds = request.Items.Select(i => i.ProductVariantId).ToList();

        var stockItems = await stockItemWriteRepository.GetByProductVariantIdsAsync(productVariantIds, cancellationToken);
        var stockItemsByVariantId = stockItems.ToDictionary(s => s.ProductVariantId);

        var missingVariantIds = productVariantIds.Where(id => !stockItemsByVariantId.ContainsKey(id)).ToList();

        if (missingVariantIds.Count > 0)
        {
            return Result.Failure(Error.NotFound(
                "StockItems.NotFound",
                $"Şu varyantlar için stok kaydı bulunamadı: {string.Join(", ", missingVariantIds)}."));
        }

        foreach (var item in request.Items)
        {
            var result = stockItemsByVariantId[item.ProductVariantId].Reserve(item.Quantity);

            if (result.IsFailure)
            {
                return result;
            }
        }

        await stockItemWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
