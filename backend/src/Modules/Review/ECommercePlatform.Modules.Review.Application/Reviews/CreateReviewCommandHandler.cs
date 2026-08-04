using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Review.Application.Abstractions;
using ECommercePlatform.SharedKernel;
using MediatR;

namespace ECommercePlatform.Modules.Review.Application.Reviews;

/// <summary>
/// A purely read-side, two-module verification chain — unlike Order's checkout orchestration
/// (which writes across modules) or Payment -> Order (a single module), this handler reads from
/// both Order (does the caller own a completed purchase?) and Catalog (does that purchase actually
/// include this product?) before writing anything itself.
///
/// Order's items carry ProductVariantId, not ProductId, so verifying "did this order include this
/// product" requires resolving variants to their parent product — reusing Catalog's existing
/// GetProductVariantSummariesQuery (already extended with ProductId for this purpose) rather than
/// adding a near-duplicate query.
/// </summary>
public sealed class CreateReviewCommandHandler(
    IReviewWriteRepository reviewWriteRepository,
    ISender sender) : ICommandHandler<CreateReviewCommand, Guid>
{
    private static readonly string[] PurchaseCompletedStatuses = ["Paid", "Preparing", "Shipped", "Delivered"];

    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (await reviewWriteRepository.ExistsByProductIdAndUserIdAsync(request.ProductId, request.UserId, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("Reviews.AlreadyReviewed", "Bu ürünü zaten değerlendirdiniz."));
        }

        var orderResult = await sender.Send(new GetOrderByIdQuery(request.OrderId), cancellationToken);

        if (orderResult.IsFailure || orderResult.Value.UserId != request.UserId)
        {
            return Result.Failure<Guid>(Error.NotFound("Reviews.OrderNotFound", "Sipariş bulunamadı."));
        }

        var order = orderResult.Value;

        if (!PurchaseCompletedStatuses.Contains(order.Status))
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Reviews.OrderNotPurchased",
                $"Bu sipariş üzerinden yorum yapılamaz (şu an: {order.Status})."));
        }

        var variantIds = order.Items.Select(i => i.ProductVariantId).ToList();
        var summariesResult = await sender.Send(new GetProductVariantSummariesQuery(variantIds), cancellationToken);

        if (summariesResult.IsFailure || summariesResult.Value.All(s => s.ProductId != request.ProductId))
        {
            return Result.Failure<Guid>(Error.Conflict(
                "Reviews.ProductNotInOrder", "Bu ürünü bu siparişte satın almadınız."));
        }

        var review = Domain.Review.Create(request.ProductId, request.UserId, request.OrderId, request.Rating, request.Comment);

        reviewWriteRepository.Add(review);
        await reviewWriteRepository.SaveChangesAsync(cancellationToken);

        return review.Id;
    }
}
