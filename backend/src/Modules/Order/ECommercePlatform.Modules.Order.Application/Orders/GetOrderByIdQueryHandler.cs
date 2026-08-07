using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Order.Application.Abstractions;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Order.Application.Orders;

/// <summary>
/// Order's own schema only stores each item's ProductVariantId, not its parent ProductId (the
/// same snapshot-vs-live split as Cart) — resolving it for the caller (e.g. Review's "which
/// products in this order can be reviewed") reuses Catalog's GetProductVariantSummariesQuery,
/// the same cross-module read composition GetCartByIdQueryHandler already does.
/// </summary>
public sealed class GetOrderByIdQueryHandler(IOrderReadRepository orderReadRepository, ISender sender)
    : IQueryHandler<GetOrderByIdQuery, OrderDetailDto>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderReadRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderDetailDto>(Error.NotFound("Orders.NotFound", "Sipariş bulunamadı."));
        }

        if (order.Items.Count == 0)
        {
            return order;
        }

        var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
        var summariesResult = await sender.Send(new GetProductVariantSummariesQuery(variantIds), cancellationToken);
        var productIdByVariantId = summariesResult.Value.ToDictionary(s => s.ProductVariantId, s => s.ProductId);

        var enrichedItems = order.Items
            .Select(i => i with { ProductId = productIdByVariantId.GetValueOrDefault(i.ProductVariantId) })
            .ToList();

        return order with { Items = enrichedItems };
    }
}
