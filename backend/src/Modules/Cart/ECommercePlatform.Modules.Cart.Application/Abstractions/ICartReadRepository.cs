using ECommercePlatform.Modules.Cart.Application.Dtos;

namespace ECommercePlatform.Modules.Cart.Application.Abstractions;

public interface ICartReadRepository
{
    /// <summary>Raw read from the cart schema only — no Catalog enrichment. See GetCartByIdQueryHandler for that.</summary>
    Task<CartRawDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
