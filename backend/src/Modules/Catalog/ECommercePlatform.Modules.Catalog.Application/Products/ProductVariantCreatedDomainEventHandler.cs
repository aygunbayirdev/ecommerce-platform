using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Domain;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using MediatR;

namespace ECommercePlatform.Modules.Catalog.Application.Products;

/// <summary>
/// Lives in Catalog (the module that raises the event), not Inventory — the source module owns
/// knowing what needs to react and calls the target module's public command via ISender. Mirrors
/// WMS's Inbound → Inventory pattern: dependency direction is Catalog.Application → Inventory.Application,
/// never the reverse.
/// </summary>
public sealed class ProductVariantCreatedDomainEventHandler(ISender sender)
    : IDomainEventHandler<ProductVariantCreatedDomainEvent>
{
    public async Task Handle(DomainEventNotification<ProductVariantCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        await sender.Send(new CreateStockItemCommand(notification.DomainEvent.ProductVariantId), cancellationToken);
    }
}
