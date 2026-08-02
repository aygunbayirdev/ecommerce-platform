using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Users;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Identity.UnitTests;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IUserWriteRepository> _userWriteRepository = new();
    private readonly Mock<IRefreshTokenWriteRepository> _refreshTokenWriteRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenGenerator> _tokenGenerator = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userWriteRepository.Object,
            _refreshTokenWriteRepository.Object,
            _passwordHasher.Object,
            _tokenGenerator.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        var user = User.Register("test@example.com", "hashed-password", "Ada", "Lovelace", null);
        var command = new LoginCommand("test@example.com", "Password123!");

        _userWriteRepository
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(true);

        _tokenGenerator
            .Setup(t => t.GenerateAccessToken(user))
            .Returns(new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15)));

        _tokenGenerator
            .Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));

        _tokenGenerator.Setup(t => t.HashRefreshToken("refresh-token")).Returns("hashed-refresh-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        _refreshTokenWriteRepository.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _refreshTokenWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        var user = User.Register("test@example.com", "hashed-password", "Ada", "Lovelace", null);
        var command = new LoginCommand("test@example.com", "WrongPassword");

        _userWriteRepository
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(command.Password, user.PasswordHash)).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var command = new LoginCommand("missing@example.com", "Password123!");

        _userWriteRepository
            .Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }
}
