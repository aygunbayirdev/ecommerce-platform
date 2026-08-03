using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed class RemoveCartItemCommandHandler(ICartWriteRepository cartWriteRepository)
    : ICommandHandler<RemoveCartItemCommand>
{
    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartWriteRepository.GetByIdWithItemsAsync(request.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(Error.NotFound("Carts.NotFound", "Sepet bulunamadı."));
        }

        var result = cart.RemoveItem(request.ProductVariantId);

        if (result.IsFailure)
        {
            return result;
        }

        await cartWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
