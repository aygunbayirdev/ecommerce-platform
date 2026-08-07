using ECommercePlatform.Modules.Cart.Application.Abstractions;
using ECommercePlatform.Modules.Cart.Application.Carts;
using ECommercePlatform.Modules.Cart.Application.Dtos;
using ECommercePlatform.Modules.Catalog.Application.Dtos;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.SharedKernel;
using MediatR;
using Moq;

namespace ECommercePlatform.Modules.Cart.UnitTests;

public sealed class GetCartByIdQueryHandlerTests
{
    private readonly Mock<ICartReadRepository> _cartReadRepository = new();
    private readonly Mock<ISender> _sender = new();
    private readonly GetCartByIdQueryHandler _handler;

    public GetCartByIdQueryHandlerTests()
    {
        _handler = new GetCartByIdQueryHandler(_cartReadRepository.Object, _sender.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCartDoesNotExist()
    {
        var cartId = Guid.NewGuid();
        _cartReadRepository
            .Setup(r => r.GetByIdAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartRawDto?)null);

        var result = await _handler.Handle(new GetCartByIdQuery(cartId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnZeroTotalWithoutCallingSender_WhenCartIsEmpty()
    {
        var cartId = Guid.NewGuid();
        var cart = new CartRawDto(cartId, Guid.NewGuid(), []);
        _cartReadRepository.Setup(r => r.GetByIdAsync(cartId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        var result = await _handler.Handle(new GetCartByIdQuery(cartId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0m, result.Value.Total);
        _sender.Verify(
            s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldEnrichItemsFromCatalog_AndComputeCorrectTotal()
    {
        var cartId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var cart = new CartRawDto(cartId, Guid.NewGuid(), [new CartRawItemDto(variantId, 3)]);
        _cartReadRepository.Setup(r => r.GetByIdAsync(cartId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        var summary = new ProductVariantSummaryDto(variantId, "Telefon", "SKU-1", 100m, "https://img", true, Guid.NewGuid());
        _sender
            .Setup(s => s.Send(It.IsAny<GetProductVariantSummariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<ProductVariantSummaryDto>>([summary]));

        var result = await _handler.Handle(new GetCartByIdQuery(cartId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("Telefon", item.ProductName);
        Assert.Equal(300m, item.LineTotal);
        Assert.Equal(300m, result.Value.Total);
    }
}
