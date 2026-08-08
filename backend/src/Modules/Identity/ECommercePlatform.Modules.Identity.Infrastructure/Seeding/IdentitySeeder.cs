using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommercePlatform.Modules.Identity.Infrastructure.Seeding;

/// <summary>
/// Ensures a default Admin user exists. Bypasses RegisterUserCommand on purpose: that command
/// always registers UserRole.Customer (no ISender-reachable way to create an Admin), so this
/// goes straight to IUserWriteRepository the same way WMS's IdentitySeeder bypasses its own
/// command pipeline. Called once from ECommercePlatform.Api's startup, after migrations have
/// already run; idempotent, so it is safe to run on every app start.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider rootServices, CancellationToken cancellationToken = default)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;

        var adminOptions = services.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        var userWriteRepository = services.GetRequiredService<IUserWriteRepository>();

        var existingAdmin = await userWriteRepository.GetByEmailAsync(adminOptions.Email, cancellationToken);

        if (existingAdmin is not null)
        {
            return;
        }

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        var admin = User.Register(
            adminOptions.Email,
            passwordHasher.Hash(adminOptions.Password),
            adminOptions.FirstName,
            adminOptions.LastName,
            phoneNumber: null,
            role: UserRole.Admin);

        userWriteRepository.Add(admin);
        await userWriteRepository.SaveChangesAsync(cancellationToken);

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("ECommercePlatform.Modules.Identity.Seeding");

        logger.LogWarning(
            "Seeded default admin user {Email}. Change the password immediately outside local development.",
            adminOptions.Email);
    }
}
