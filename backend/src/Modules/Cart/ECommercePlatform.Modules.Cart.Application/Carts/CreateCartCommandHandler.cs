using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed class CreateCartCommandHandler(ICartWriteRepository cartWriteRepository)
    : ICommandHandler<CreateCartCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var cart = Domain.Cart.Create(request.UserId);

        cartWriteRepository.Add(cart);
        await cartWriteRepository.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
