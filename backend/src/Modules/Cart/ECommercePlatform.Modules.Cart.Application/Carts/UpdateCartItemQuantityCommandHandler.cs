using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed class UpdateCartItemQuantityCommandHandler(ICartWriteRepository cartWriteRepository)
    : ICommandHandler<UpdateCartItemQuantityCommand>
{
    public async Task<Result> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartWriteRepository.GetByIdWithItemsAsync(request.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(Error.NotFound("Carts.NotFound", "Sepet bulunamadı."));
        }

        var result = cart.UpdateItemQuantity(request.ProductVariantId, request.Quantity);

        if (result.IsFailure)
        {
            return result;
        }

        await cartWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
