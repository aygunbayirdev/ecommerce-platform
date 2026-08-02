using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed class RegisterUserCommandHandler(
    IUserWriteRepository userWriteRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await userWriteRepository.ExistsByEmailAsync(request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<Guid>(Error.Conflict("Users.EmailAlreadyExists", "Bu e-posta adresi zaten kayıtlı."));
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Register(request.Email, passwordHash, request.FirstName, request.LastName, request.PhoneNumber);

        userWriteRepository.Add(user);
        await userWriteRepository.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
