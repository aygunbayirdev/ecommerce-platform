using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Dtos;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.Application.Users;

public sealed class GetUserByIdQueryHandler(IUserReadRepository userReadRepository)
    : IQueryHandler<GetUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userReadRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("Users.NotFound", "Kullanıcı bulunamadı."));
        }

        return user;
    }
}
