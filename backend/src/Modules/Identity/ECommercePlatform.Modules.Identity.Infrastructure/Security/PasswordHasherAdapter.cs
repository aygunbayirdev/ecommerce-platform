using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Security;

internal sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password) => _passwordHasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
