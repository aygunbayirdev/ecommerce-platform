using ECommercePlatform.SharedKernel;

namespace ECommercePlatform.Modules.Review.Domain;

public sealed class Review : BaseEntity
{
    private Review()
    {
    }

    /// <summary>Cross-module reference to Catalog.Product — no DB foreign key by design (modular monolith: no cross-schema FKs).</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Cross-module reference to Identity.User — no DB foreign key by design.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Cross-module reference to Order.Order — the purchase this review is verified against.</summary>
    public Guid OrderId { get; private set; }

    public int Rating { get; private set; }

    public string Comment { get; private set; } = string.Empty;

    public bool IsApproved { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Review Create(Guid productId, Guid userId, Guid orderId, int rating, string comment)
    {
        return new Review
        {
            ProductId = Guard.AgainstEmpty(productId, nameof(productId)),
            UserId = Guard.AgainstEmpty(userId, nameof(userId)),
            OrderId = Guard.AgainstEmpty(orderId, nameof(orderId)),
            Rating = rating,
            Comment = Guard.AgainstNullOrWhiteSpace(comment, nameof(comment)),
            IsApproved = false,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    public Result Approve()
    {
        if (IsApproved)
        {
            return Result.Failure(Error.Conflict("Reviews.AlreadyApproved", "Yorum zaten onaylanmış."));
        }

        IsApproved = true;

        return Result.Success();
    }
}
