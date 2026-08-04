using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Application.Payments;
using Moq;

namespace ECommercePlatform.Modules.Payment.UnitTests;

public sealed class CreatePendingPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentWriteRepository> _paymentWriteRepository = new();
    private readonly CreatePendingPaymentCommandHandler _handler;

    public CreatePendingPaymentCommandHandlerTests()
    {
        _handler = new CreatePendingPaymentCommandHandler(_paymentWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreatePendingPayment_WhenNoneExistsForOrder()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreatePendingPaymentCommand(orderId, userId, 250m);

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Payment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _paymentWriteRepository.Verify(r => r.Add(It.Is<Domain.Payment>(p =>
            p.OrderId == orderId && p.UserId == userId && p.Amount == 250m)), Times.Once);
        _paymentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingId_WhenPaymentAlreadyExistsForOrder_Idempotent()
    {
        var orderId = Guid.NewGuid();
        var existing = Domain.Payment.Create(orderId, Guid.NewGuid(), 250m);
        var command = new CreatePendingPaymentCommand(orderId, Guid.NewGuid(), 250m);

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value);
        _paymentWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Payment>()), Times.Never);
        _paymentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
