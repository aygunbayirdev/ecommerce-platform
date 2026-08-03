using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Inventory.Application.Abstractions;
using ECommercePlatform.Modules.Inventory.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Inventory.Application.StockItems;

/// <summary>
/// Idempotent by design (unlike most Create handlers in this codebase, which fail with Conflict
/// on a duplicate): this command is the target of an at-least-once outbox event delivery
/// (Catalog's ProductVariantCreatedDomainEvent), so a redelivered message must be a harmless
/// no-op rather than an error that burns through the outbox retry budget.
/// </summary>
public sealed class CreateStockItemCommandHandler(IStockItemWriteRepository stockItemWriteRepository)
    : ICommandHandler<CreateStockItemCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStockItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await stockItemWriteRepository.GetByProductVariantIdAsync(request.ProductVariantId, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var stockItem = StockItem.Create(request.ProductVariantId);

        stockItemWriteRepository.Add(stockItem);
        await stockItemWriteRepository.SaveChangesAsync(cancellationToken);

        return stockItem.Id;
    }
}
