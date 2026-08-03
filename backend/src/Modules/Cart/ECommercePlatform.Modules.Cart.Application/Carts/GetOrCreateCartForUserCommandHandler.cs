using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed class GetOrCreateCartForUserCommandHandler(ICartWriteRepository cartWriteRepository)
    : ICommandHandler<GetOrCreateCartForUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(GetOrCreateCartForUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await cartWriteRepository.GetByUserIdWithItemsAsync(request.UserId, cancellationToken);

        if (existing is not null)
        {
            return existing.Id;
        }

        var cart = Domain.Cart.Create(request.UserId);

        cartWriteRepository.Add(cart);
        await cartWriteRepository.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
