using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Users;
using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Identity.UnitTests;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserWriteRepository> _userWriteRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(_userWriteRepository.Object, _passwordHasher.Object);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenEmailDoesNotExist()
    {
        var command = new RegisterUserCommand("test@example.com", "Password123!", "Ada", "Lovelace", null);

        _userWriteRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasher.Setup(h => h.Hash(command.Password)).Returns("hashed-password");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _userWriteRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
        _userWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var command = new RegisterUserCommand("test@example.com", "Password123!", "Ada", "Lovelace", null);

        _userWriteRepository
            .Setup(r => r.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _userWriteRepository.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
    }
}
