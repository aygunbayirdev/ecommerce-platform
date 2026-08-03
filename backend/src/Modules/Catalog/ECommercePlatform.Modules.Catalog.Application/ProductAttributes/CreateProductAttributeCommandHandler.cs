using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Abstractions;
using ECommercePlatform.SharedKernel;
using CatalogDomain = ECommercePlatform.Modules.Catalog.Domain;

namespace ECommercePlatform.Modules.Catalog.Application.ProductAttributes;

public sealed class CreateProductAttributeCommandHandler(IProductAttributeWriteRepository productAttributeWriteRepository)
    : ICommandHandler<CreateProductAttributeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductAttributeCommand request, CancellationToken cancellationToken)
    {
        var attribute = CatalogDomain.ProductAttribute.Create(request.Name);

        productAttributeWriteRepository.Add(attribute);
        await productAttributeWriteRepository.SaveChangesAsync(cancellationToken);

        return attribute.Id;
    }
}
