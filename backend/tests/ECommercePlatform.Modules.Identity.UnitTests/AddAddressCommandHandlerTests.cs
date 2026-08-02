using ECommercePlatform.Modules.Identity.Application.Abstractions;
using ECommercePlatform.Modules.Identity.Application.Addresses;
using ECommercePlatform.Modules.Identity.Domain;
using Moq;

namespace ECommercePlatform.Modules.Identity.UnitTests;

public sealed class AddAddressCommandHandlerTests
{
    private readonly Mock<IUserWriteRepository> _userWriteRepository = new();
    private readonly AddAddressCommandHandler _handler;

    public AddAddressCommandHandlerTests()
    {
        _handler = new AddAddressCommandHandler(_userWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddAddress_WhenUserExists()
    {
        var user = User.Register("test@example.com", "hashed-password", "Ada", "Lovelace", null);
        var command = new AddAddressCommand(
            user.Id, "Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710", true);

        _userWriteRepository
            .Setup(r => r.GetByIdWithAddressesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.Addresses);
        _userWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var command = new AddAddressCommand(
            Guid.NewGuid(), "Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710", true);

        _userWriteRepository
            .Setup(r => r.GetByIdWithAddressesAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        _userWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
