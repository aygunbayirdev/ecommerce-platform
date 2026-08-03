using FluentValidation;

namespace ECommercePlatform.Modules.Cart.Application.Carts;

public sealed class GetOrCreateCartForUserCommandValidator : AbstractValidator<GetOrCreateCartForUserCommand>
{
    public GetOrCreateCartForUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
