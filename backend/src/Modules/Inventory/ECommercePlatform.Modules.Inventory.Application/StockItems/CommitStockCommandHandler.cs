using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

/// <summary>All-or-nothing, same rationale as ReserveStockCommandHandler.</summary>
public sealed class CommitStockCommandHandler(IStockItemWriteRepository stockItemWriteRepository)
    : ICommandHandler<CommitStockCommand>
{
    public async Task<Result> Handle(CommitStockCommand request, CancellationToken cancellationToken)
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
            var result = stockItemsByVariantId[item.ProductVariantId].Commit(item.Quantity);

            if (result.IsFailure)
            {
                return result;
            }
        }

        await stockItemWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
