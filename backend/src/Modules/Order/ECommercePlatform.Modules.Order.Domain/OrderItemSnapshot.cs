namespace ECommercePlatform.Modules.Order.Domain;

/// <summary>Factory input for Order.Create — the checkout handler builds these from Cart's already-enriched view, not from a fresh Catalog call.</summary>
public sealed record OrderItemSnapshot(Guid ProductVariantId, string ProductName, string Sku, decimal UnitPrice, int Quantity);
