using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed class ClearCartCommandHandler(ICartWriteRepository cartWriteRepository)
    : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartWriteRepository.GetByIdWithItemsAsync(request.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Failure(Error.NotFound("Carts.NotFound", "Sepet bulunamadı."));
        }

        cart.Clear();

        await cartWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
