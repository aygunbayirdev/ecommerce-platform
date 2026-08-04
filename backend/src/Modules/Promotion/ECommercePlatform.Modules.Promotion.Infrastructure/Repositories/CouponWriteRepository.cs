using ECommercePlatform.Modules.Promotion.Application.Abstractions;
using ECommercePlatform.Modules.Promotion.Domain;
using ECommercePlatform.Modules.Promotion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Modules.Promotion.Infrastructure.Repositories;

internal sealed class CouponWriteRepository(PromotionDbContext dbContext) : ICouponWriteRepository
{
    public Task<Coupon?> GetByIdWithRedemptionsAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Coupons.Include(c => c.Redemptions).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Coupon?> GetByCodeWithRedemptionsAsync(string code, CancellationToken cancellationToken)
        => dbContext.Coupons.Include(c => c.Redemptions)
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);

    public Task<Coupon?> GetByRedeemedOrderIdAsync(Guid orderId, CancellationToken cancellationToken)
        => dbContext.Coupons.Include(c => c.Redemptions)
            .FirstOrDefaultAsync(c => c.Redemptions.Any(r => r.OrderId == orderId), cancellationToken);

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken)
        => dbContext.Coupons.AnyAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);

    public void Add(Coupon coupon) => dbContext.Coupons.Add(coupon);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
