using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.Modules.Review.Application.Reviews;
using ECommercePlatform.SharedKernel;
using Moq;

namespace ECommercePlatform.Modules.Review.UnitTests;

public sealed class RejectReviewCommandHandlerTests
{
    private readonly Mock<IReviewWriteRepository> _reviewWriteRepository = new();
    private readonly RejectReviewCommandHandler _handler;

    public RejectReviewCommandHandlerTests()
    {
        _handler = new RejectReviewCommandHandler(_reviewWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenReviewDoesNotExist()
    {
        _reviewWriteRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Review?)null);

        var result = await _handler.Handle(new RejectReviewCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        _reviewWriteRepository.Verify(r => r.Remove(It.IsAny<Domain.Review>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRemoveAndSave_OnHappyPath()
    {
        var review = Domain.Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, "Kötü ürün");

        _reviewWriteRepository
            .Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var result = await _handler.Handle(new RejectReviewCommand(review.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _reviewWriteRepository.Verify(r => r.Remove(review), Times.Once);
        _reviewWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
