using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Application.Payments;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Payment.UnitTests;

public sealed class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentWriteRepository> _paymentWriteRepository = new();
    private readonly Mock<IPaymentGateway> _paymentGateway = new();
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _handler = new ProcessPaymentCommandHandler(_paymentWriteRepository.Object, _paymentGateway.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderReadyForPaymentEventHasNotArrivedYet()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), "4111111111111111", "key-1");

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Payment?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Payments.OrderNotReady", result.Error.Code);
        _paymentGateway.Verify(g => g.ChargeAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPaymentBelongsToAnotherUser()
    {
        var payment = Domain.Payment.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var command = new ProcessPaymentCommand(Guid.NewGuid(), payment.OrderId, "4111111111111111", "key-1");

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Payments.OrderNotFound", result.Error.Code);
        _paymentGateway.Verify(g => g.ChargeAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenPaymentAlreadySucceeded()
    {
        var userId = Guid.NewGuid();
        var payment = Domain.Payment.Create(Guid.NewGuid(), userId, 100m);
        payment.Attempt("previous-key", isSuccessful: true, failureReason: null);
        var command = new ProcessPaymentCommand(userId, payment.OrderId, "4111111111111111", "new-key");

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _paymentGateway
            .Setup(g => g.ChargeAsync(100m, command.CardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeResult(IsSuccessful: true, FailureReason: null));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("Payments.AlreadyPaid", result.Error.Code);
        _paymentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenChargeSucceeds()
    {
        var userId = Guid.NewGuid();
        var payment = Domain.Payment.Create(Guid.NewGuid(), userId, 250m);
        var command = new ProcessPaymentCommand(userId, payment.OrderId, "4111111111111111", "key-1");

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _paymentGateway
            .Setup(g => g.ChargeAsync(250m, command.CardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeResult(IsSuccessful: true, FailureReason: null));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Domain.PaymentStatus.Succeeded, payment.Status);
        Assert.Contains(payment.DomainEvents, e => e is ECommercePlatform.IntegrationEvents.PaymentSucceededIntegrationEvent);
        _paymentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_AndLeavePaymentPending_WhenChargeDeclined()
    {
        var userId = Guid.NewGuid();
        var payment = Domain.Payment.Create(Guid.NewGuid(), userId, 250m);
        var command = new ProcessPaymentCommand(userId, payment.OrderId, "4111111111110000", "key-1");

        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);
        _paymentGateway
            .Setup(g => g.ChargeAsync(250m, command.CardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeResult(IsSuccessful: false, FailureReason: "Kart reddedildi (mock)."));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal("Payments.ChargeDeclined", result.Error.Code);
        Assert.Equal(Domain.PaymentStatus.Pending, payment.Status);
        _paymentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
