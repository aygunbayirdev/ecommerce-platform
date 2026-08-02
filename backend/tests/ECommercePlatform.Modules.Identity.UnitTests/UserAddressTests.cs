using ECommercePlatform.Modules.Identity.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Identity.UnitTests;

public sealed class UserAddressTests
{
    private static User CreateUser() => User.Register("test@example.com", "hashed-password", "Ada", "Lovelace", null);

    [Fact]
    public void AddAddress_FirstAddress_BecomesDefaultAutomatically()
    {
        var user = CreateUser();

        var address = user.AddAddress("Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710", isDefault: false);

        Assert.True(address.IsDefault);
    }

    [Fact]
    public void AddAddress_WithIsDefaultTrue_UnsetsExistingDefault()
    {
        var user = CreateUser();
        var first = user.AddAddress("Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710", isDefault: false);

        var second = user.AddAddress("İş", "Ada Lovelace", "5551234567", "İstanbul", "Şişli", "Bomonti Cd. No:2", "34380", isDefault: true);

        Assert.False(user.Addresses.Single(a => a.Id == first.Id).IsDefault);
        Assert.True(second.IsDefault);
    }

    [Fact]
    public void SetDefaultAddress_SwitchesDefaultCorrectly()
    {
        var user = CreateUser();
        var first = user.AddAddress("Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710", isDefault: false);
        var second = user.AddAddress("İş", "Ada Lovelace", "5551234567", "İstanbul", "Şişli", "Bomonti Cd. No:2", "34380", isDefault: false);

        var result = user.SetDefaultAddress(second.Id);

        Assert.True(result.IsSuccess);
        Assert.False(user.Addresses.Single(a => a.Id == first.Id).IsDefault);
        Assert.True(user.Addresses.Single(a => a.Id == second.Id).IsDefault);
    }

    [Fact]
    public void RemoveAddress_WhenRemovingDefault_PromotesAnotherAddress()
    {
        var user = CreateUser();
        var first = user.AddAddress("Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710", isDefault: false);
        user.AddAddress("İş", "Ada Lovelace", "5551234567", "İstanbul", "Şişli", "Bomonti Cd. No:2", "34380", isDefault: false);

        var result = user.RemoveAddress(first.Id);

        Assert.True(result.IsSuccess);
        Assert.Single(user.Addresses);
        Assert.True(user.Addresses.Single().IsDefault);
    }

    [Fact]
    public void UpdateAddress_UnknownId_ReturnsNotFound()
    {
        var user = CreateUser();

        var result = user.UpdateAddress(Guid.NewGuid(), "Ev", "Ada Lovelace", "5551234567", "İstanbul", "Kadıköy", "Moda Cd. No:1", "34710");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
