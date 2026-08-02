using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed class AddAddressCommandHandler(IUserWriteRepository userWriteRepository)
    : ICommandHandler<AddAddressCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await userWriteRepository.GetByIdWithAddressesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Users.NotFound", "Kullanıcı bulunamadı."));
        }

        var address = user.AddAddress(
            request.Title,
            request.RecipientName,
            request.PhoneNumber,
            request.City,
            request.District,
            request.FullAddressLine,
            request.PostalCode,
            request.IsDefault);

        await userWriteRepository.SaveChangesAsync(cancellationToken);

        return address.Id;
    }
}
