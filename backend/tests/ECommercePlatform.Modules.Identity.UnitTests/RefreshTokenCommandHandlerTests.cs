using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Users;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Identity.UnitTests;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserWriteRepository> _userWriteRepository = new();
    private readonly Mock<IRefreshTokenWriteRepository> _refreshTokenWriteRepository = new();
    private readonly Mock<ITokenGenerator> _tokenGenerator = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(
            _userWriteRepository.Object, _refreshTokenWriteRepository.Object, _tokenGenerator.Object);
        _tokenGenerator.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns("hashed-token");
    }

    private static User CreateActiveUser()
    {
        return User.Register("customer@example.com", "hashed-password", "Ayşe", "Yılmaz", null);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenTokenHashIsNotFound()
    {
        _refreshTokenWriteRepository
            .Setup(r => r.GetByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(new RefreshTokenCommand("some-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenTokenIsRevoked()
    {
        var refreshToken = RefreshToken.Issue(Guid.NewGuid(), "hashed-token", DateTime.UtcNow.AddDays(7));
        refreshToken.Revoke();

        _refreshTokenWriteRepository
            .Setup(r => r.GetByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result = await _handler.Handle(new RefreshTokenCommand("some-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        _userWriteRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenUserIsInactive()
    {
        var user = CreateActiveUser();
        user.Deactivate();
        var refreshToken = RefreshToken.Issue(user.Id, "hashed-token", DateTime.UtcNow.AddDays(7));

        _refreshTokenWriteRepository
            .Setup(r => r.GetByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _userWriteRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(new RefreshTokenCommand("some-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        _refreshTokenWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRevokeOldTokenAndIssueNewOne_OnHappyPath()
    {
        var user = CreateActiveUser();
        var refreshToken = RefreshToken.Issue(user.Id, "hashed-token", DateTime.UtcNow.AddDays(7));
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(15);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        _refreshTokenWriteRepository
            .Setup(r => r.GetByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _userWriteRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenGenerator.Setup(t => t.GenerateAccessToken(user)).Returns(new AccessTokenResult("new-access-token", accessTokenExpiry));
        _tokenGenerator.Setup(t => t.GenerateRefreshToken()).Returns(new RefreshTokenResult("new-refresh-token", refreshTokenExpiry));

        var result = await _handler.Handle(new RefreshTokenCommand("some-token"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(refreshToken.IsActive);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
        _refreshTokenWriteRepository.Verify(
            r => r.Add(It.Is<RefreshToken>(t => t.UserId == user.Id && t.TokenHash == "hashed-token")),
            Times.Once);
        _refreshTokenWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
