using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Order.Application.Dtos;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Application.Reviews;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Review.UnitTests;

public sealed class CreateReviewCommandHandlerTests
{
    private readonly Mock<IReviewWriteRepository> _reviewWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _handler = new CreateReviewCommandHandler(_reviewWriteRepository.Object, _sender.Object);
    }

    private static OrderDetailDto Order(Guid userId, string status, Guid variantId) => new(
        Guid.NewGuid(), "ORD-1", userId, status, "Ayşe Yılmaz", "5551234567",
        "İstanbul", "Kadıköy", "Bir sokak No:1", "34000", null, 0m, DateTime.UtcNow,
        [new OrderItemDto(variantId, "Telefon", "SKU-1", 100m, 1, 100m)], [], 100m);

    private static ProductVariantSummaryDto Summary(Guid variantId, Guid productId)
        => new(variantId, "Telefon", "SKU-1", 100m, null, true, productId);

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenAlreadyReviewed()
    {
        var command = new CreateReviewCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Harika");

        _reviewWriteRepository
            .Setup(r => r.ExistsByProductIdAndUserIdAsync(command.ProductId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _sender.Verify(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenOrderDoesNotBelongToUser()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var command = new CreateReviewCommand(userId, Guid.NewGuid(), Guid.NewGuid(), 5, "Harika");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(Guid.NewGuid(), "Paid", variantId)));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _reviewWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Review>()), Times.Never);
    }

    [Theory]
    [InlineData("Created")]
    [InlineData("PaymentPending")]
    [InlineData("Cancelled")]
    public async Task Handle_ShouldReturnConflict_WhenOrderIsNotACompletedPurchase(string status)
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var command = new CreateReviewCommand(userId, Guid.NewGuid(), Guid.NewGuid(), 5, "Harika");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(userId, status, variantId)));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenProductWasNotInTheOrder()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var command = new CreateReviewCommand(userId, Guid.NewGuid(), Guid.NewGuid(), 5, "Harika");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(userId, "Delivered", variantId)));
        _sender
            .Setup(s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductVariantSummaryDto>>([Summary(variantId, Guid.NewGuid())]));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _reviewWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Review>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAddReviewAndReturnId_OnHappyPath()
    {
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new CreateReviewCommand(userId, productId, Guid.NewGuid(), 5, "Harika");

        _sender
            .Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(Order(userId, "Delivered", variantId)));
        _sender
            .Setup(s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductVariantSummaryDto>>([Summary(variantId, productId)]));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _reviewWriteRepository.Verify(r => r.Add(It.IsAny<Domain.Review>()), Times.Once);
        _reviewWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
