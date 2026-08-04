using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Payment.Application.Abstractions;
using ECommercePlatform.Modules.Payment.Application.Payments;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Payment.UnitTests;

public sealed class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IPaymentWriteRepository> _paymentWriteRepository = new();
    private readonly Mock<IPaymentGateway> _paymentGateway = new();
    private readonly Mock<ISender> _sender = new();
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _handler = new ProcessPaymentCommandHandler(_paymentWriteRepository.Object, _paymentGateway.Object, _sender.Object);
    }

    private static OrderDetailDto Order(Guid userId, string status, decimal total) => new(
        Guid.NewGuid(), "ORD-1", userId, status, "Ayşe Yılmaz", "5551234567",
        "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", null, 0m, DateTime.UtcNow, [], [], total);

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotBelongToUser()
    {
        var command = new ProcessPaymentCommand(Guid.NewGuid(), Guid.NewGuid(), "4111111111111111", "key-1");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(Guid.NewGuid(), "PaymentPending", 100m)));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _paymentGateway.Verify(g => g.ChargeAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenOrderNotPaymentPending()
    {
        var userId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(userId, Guid.NewGuid(), "4111111111111111", "key-1");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(userId, "Paid", 100m)));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _paymentGateway.Verify(g => g.ChargeAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkOrderAsPaid_WhenChargeSucceeds()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(userId, orderId, "4111111111111111", "key-1");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(userId, "PaymentPending", 250m)));
        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Payment?)null);
        _paymentGateway
            .Setup(g => g.ChargeAsync(250m, command.CardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeResult(IsSuccessful: true, FailureReason: null));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _paymentWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Payment>()), Times.Once);
        _sender.Verify(s => s.Send(It.Is<MarkOrderAsPaidCommand>(c => c.OrderId == orderId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_AndNotMarkOrderPaid_WhenChargeDeclined()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(userId, orderId, "4111111111110000", "key-1");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(userId, "PaymentPending", 250m)));
        _paymentWriteRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Payment?)null);
        _paymentGateway
            .Setup(g => g.ChargeAsync(250m, command.CardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChargeResult(IsSuccessful: false, FailureReason: "Kart reddedildi (mock)."));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _paymentWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(s => s.Send(It.IsAny<MarkOrderAsPaidCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
