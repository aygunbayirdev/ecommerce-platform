using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Addresses;

public sealed class UpdateAddressCommandHandler(IUserWriteRepository userWriteRepository)
    : ICommandHandler<UpdateAddressCommand>
{
    public async Task<Result> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await userWriteRepository.GetByIdWithAddressesAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("Users.NotFound", "Kullanıcı bulunamadı."));
        }

        var result = user.UpdateAddress(
            request.AddressId,
            request.Title,
            request.RecipientName,
            request.PhoneNumber,
            request.City,
            request.District,
            request.FullAddressLine,
            request.PostalCode);

        if (result.IsFailure)
        {
            return result;
        }

        await userWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
