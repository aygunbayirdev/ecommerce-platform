using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Cart.UnitTests;

public sealed class CartTests
{
    [Fact]
    public void AddItem_ShouldAddNewLine_WhenVariantNotAlreadyInCart()
    {
        var cart = Domain.Cart.Create(userId: null);
        var variantId = Guid.NewGuid();

        cart.AddItem(variantId, 2);

        var item = Assert.Single(cart.Items);
        Assert.Equal(variantId, item.ProductVariantId);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void AddItem_ShouldIncreaseQuantity_WhenSameVariantAddedTwice()
    {
        var cart = Domain.Cart.Create(userId: null);
        var variantId = Guid.NewGuid();

        cart.AddItem(variantId, 2);
        cart.AddItem(variantId, 3);

        var item = Assert.Single(cart.Items);
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateQuantity_WhenItemExists()
    {
        var cart = Domain.Cart.Create(userId: null);
        var variantId = Guid.NewGuid();
        cart.AddItem(variantId, 2);

        var result = cart.UpdateItemQuantity(variantId, 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, cart.Items.Single().Quantity);
    }

    [Fact]
    public void UpdateItemQuantity_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var cart = Domain.Cart.Create(userId: null);

        var result = cart.UpdateItemQuantity(Guid.NewGuid(), 10);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public void RemoveItem_ShouldRemove_WhenItemExists()
    {
        var cart = Domain.Cart.Create(userId: null);
        var variantId = Guid.NewGuid();
        cart.AddItem(variantId, 2);

        var result = cart.RemoveItem(variantId);

        Assert.True(result.IsSuccess);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public void RemoveItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var cart = Domain.Cart.Create(userId: null);

        var result = cart.RemoveItem(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
