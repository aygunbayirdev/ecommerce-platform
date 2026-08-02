using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed class SetDefaultAddressCommandHandler(IUserWriteRepository userWriteRepository)
    : ICommandHandler<SetDefaultAddressCommand>
{
    public async Task<Result> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await userWriteRepository.GetByIdWithAddressesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("Users.NotFound", "Kullanıcı bulunamadı."));
        }

        var result = user.SetDefaultAddress(request.AddressId);

        if (result.IsFailure)
        {
            return result;
        }

        await userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
