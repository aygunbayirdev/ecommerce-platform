using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.Modules.Catalog.Application.Categories;
using ECommercePlatform.Modules.Catalog.Domain;
using Moq;

namespace ECommercePlatform.Modules.Catalog.UnitTests;

public sealed class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryWriteRepository> _categoryWriteRepository = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_categoryWriteRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenNoParent()
    {
        var command = new CreateCategoryCommand("Elektronik", ParentCategoryId: null, DisplayOrder: 0);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        _categoryWriteRepository.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
        _categoryWriteRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenParentDoesNotExist()
    {
        var parentId = Guid.NewGuid();
        var command = new CreateCategoryCommand("Telefon", parentId, DisplayOrder: 0);

        _categoryWriteRepository
            .Setup(r => r.GetByIdAsync(parentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        _categoryWriteRepository.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);
    }
}
