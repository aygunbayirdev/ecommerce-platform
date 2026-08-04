using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Application.Reviews;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Review.UnitTests;

public sealed class ApproveReviewCommandHandlerTests
{
    private readonly Mock<IReviewWriteRepository> _reviewWriteRepository = new();
    private readonly ApproveReviewCommandHandler _handler;

    public ApproveReviewCommandHandlerTests()
    {
        _handler = new ApproveReviewCommandHandler(_reviewWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenReviewDoesNotExist()
    {
        _reviewWriteRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Review?)null);

        var result = await _handler.Handle(new ApproveReviewCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflictWithoutSaving_WhenAlreadyApproved()
    {
        var review = Domain.Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Harika");
        review.Approve();

        _reviewWriteRepository
            .Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var result = await _handler.Handle(new ApproveReviewCommand(review.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        _reviewWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldApproveAndSave_OnHappyPath()
    {
        var review = Domain.Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, "Harika");

        _reviewWriteRepository
            .Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var result = await _handler.Handle(new ApproveReviewCommand(review.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(review.IsApproved);
        _reviewWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
