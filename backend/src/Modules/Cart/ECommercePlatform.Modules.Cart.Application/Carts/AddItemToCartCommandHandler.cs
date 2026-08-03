using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

/// <summary>
/// Validates the variant against Catalog via a direct, synchronous query call (ISender), not an
/// event — this is a read composition that needs an immediate, consistent answer ("does this
/// variant exist and is it active right now"), unlike the Catalog → Inventory domain event flow
/// which tolerates eventual consistency for a state change.
/// </summary>
public sealed class AddItemToCartCommandHandler(
    ICartWriteRepository cartWriteRepository,
    ISender sender) : ICommandHandler<AddItemToCartCommand>
{
    public async Task<Result> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartWriteRepository.GetByIdWithItemsAsync(request.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(Error.NotFound("Carts.NotFound", "Sepet bulunamadı."));
        }

        var summariesResult = await sender.Send(
            new GetProductVariantSummariesQuery([request.ProductVariantId]), cancellationToken);

        var summary = summariesResult.Value.FirstOrDefault();

        if (summary is null)
        {
            return Result.Failure(Error.NotFound("Carts.ProductVariantNotFound", "Ürün varyantı bulunamadı."));
        }

        if (!summary.IsActive)
        {
            return Result.Failure(Error.Conflict("Carts.ProductVariantInactive", "Bu ürün varyantı artık satışta değil."));
        }

        cart.AddItem(request.ProductVariantId, request.Quantity);

        await cartWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
