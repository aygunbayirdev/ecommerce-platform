using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.Modules.Cart.Application.Carts;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Cart.UnitTests;

public sealed class AddItemToCartCommandHandlerTests
{
    private readonly Mock<ICartWriteRepository> _cartWriteRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly AddItemToCartCommandHandler _handler;

    public AddItemToCartCommandHandlerTests()
    {
        _handler = new AddItemToCartCommandHandler(_cartWriteRepository.Object, _sender.Object);
    }

    private static ProductVariantSummaryDto Summary(Guid variantId, bool isActive = true)
        => new(variantId, "Telefon", "SKU-1", 100m, null, isActive);

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        var command = new AddItemToCartCommand(Guid.NewGuid(), Guid.NewGuid(), 1);

        _cartWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(command.CartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Cart?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _sender.Verify(
            s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProductVariantNotFoundInCatalog()
    {
        var cart = Domain.Cart.Create(userId: null);
        var command = new AddItemToCartCommand(cart.Id, Guid.NewGuid(), 1);

        _cartWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(cart.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        _sender
            .Setup(s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductVariantSummaryDto>>([]));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenProductVariantIsInactive()
    {
        var cart = Domain.Cart.Create(userId: null);
        var variantId = Guid.NewGuid();
        var command = new AddItemToCartCommand(cart.Id, variantId, 1);

        _cartWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(cart.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        _sender
            .Setup(s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductVariantSummaryDto>>([Summary(variantId, isActive: false)]));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task Handle_ShouldAddItem_WhenVariantExistsAndActive()
    {
        var cart = Domain.Cart.Create(userId: null);
        var variantId = Guid.NewGuid();
        var command = new AddItemToCartCommand(cart.Id, variantId, 3);

        _cartWriteRepository
            .Setup(r => r.GetByIdWithItemsAsync(cart.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
        _sender
            .Setup(s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductVariantSummaryDto>>([Summary(variantId)]));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(cart.Items);
        Assert.Equal(variantId, item.ProductVariantId);
        Assert.Equal(3, item.Quantity);
        _cartWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
