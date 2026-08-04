using ECommercePlatform.Modules.Review.Domain;
using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Review.UnitTests;

public sealed class ReviewTests
{
    [Fact]
    public void Create_ShouldStartAsUnapproved()
    {
        var review = Domain.Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Harika ürün!");

        Assert.False(review.IsApproved);
    }

    [Fact]
    public void Approve_ShouldSucceed_WhenNotYetApproved()
    {
        var review = Domain.Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Beğendim");

        var result = review.Approve();

        Assert.True(result.IsSuccess);
        Assert.True(review.IsApproved);
    }

    [Fact]
    public void Approve_ShouldReturnConflict_WhenCalledTwice()
    {
        var review = Domain.Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 4, "Beğendim");
        review.Approve();

        var result = review.Approve();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
