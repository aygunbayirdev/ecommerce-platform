using ECommercePlatform.BuildingBlocks.Application.Messaging;
using ECommercePlatform.Modules.Cart.Application.Carts;
using ECommercePlatform.Modules.Catalog.Application.Brands;
using ECommercePlatform.Modules.Catalog.Application.Categories;
using ECommercePlatform.Modules.Catalog.Application.ProductAttributes;
using ECommercePlatform.Modules.Catalog.Application.Products;
using ECommercePlatform.Modules.Identity.Application.Addresses;
using ECommercePlatform.Modules.Identity.Application.Users;
using ECommercePlatform.Modules.Inventory.Application.StockItems;
using ECommercePlatform.Modules.Order.Application.Orders;
using ECommercePlatform.Modules.Promotion.Application.Coupons;
using ECommercePlatform.Modules.Promotion.Domain;
using ECommercePlatform.Modules.Review.Application.Reviews;
using ECommercePlatform.Modules.Shipping.Application.Shipments;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommercePlatform.Api.Seeding;

/// <summary>
/// Populates a brand-new deployment with a small, internally consistent demo dataset so a
/// reviewer sees a populated, working storefront (products with real photos, stock, coupons,
/// customer accounts with order history in different lifecycle stages, an approved review)
/// instead of an empty shell. Every value flows through the same command handlers a real HTTP
/// call would hit (CreateProductCommand, MarkOrderAsPaidCommand, etc.) - mirrors WMS's
/// WMS.Api/Seeding/DemoDataSeeder.cs, adapted to this project's modules.
///
/// Idempotent and non-destructive: bails out immediately if any product already exists, whether
/// that's this seeder having already run once, or a real deployment with real data. Toggle via
/// appsettings' Seeding:SeedDemoData (default true).
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoPassword = "Demo123!";

    public static async Task SeedAsync(IServiceProvider rootServices, CancellationToken cancellationToken = default)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;
        var sender = services.GetRequiredService<ISender>();

        var existingProducts = await sender.Send(new GetProductsQuery(null, 1, 1), cancellationToken);
        if (existingProducts.Value.TotalCount > 0)
        {
            return;
        }

        // --- Catalog: reference data --------------------------------------------
        var elektronik = await SendForIdAsync(sender, new CreateCategoryCommand("Elektronik", null, 1), cancellationToken);
        var bilgisayarAksesuar = await SendForIdAsync(sender, new CreateCategoryCommand("Bilgisayar & Aksesuar", elektronik, 1), cancellationToken);
        var giyim = await SendForIdAsync(sender, new CreateCategoryCommand("Giyim", null, 2), cancellationToken);
        var evYasam = await SendForIdAsync(sender, new CreateCategoryCommand("Ev & Yaşam", null, 3), cancellationToken);
        var sporOutdoor = await SendForIdAsync(sender, new CreateCategoryCommand("Spor & Outdoor", null, 4), cancellationToken);

        var beden = await SendForIdAsync(sender, new CreateProductAttributeCommand("Beden"), cancellationToken);
        var renk = await SendForIdAsync(sender, new CreateProductAttributeCommand("Renk"), cancellationToken);
        await SendAsync(sender, new AssignCategoryAttributeCommand(giyim, beden), cancellationToken);
        await SendAsync(sender, new AssignCategoryAttributeCommand(giyim, renk), cancellationToken);
        await SendAsync(sender, new AssignCategoryAttributeCommand(sporOutdoor, beden), cancellationToken);
        await SendAsync(sender, new AssignCategoryAttributeCommand(sporOutdoor, renk), cancellationToken);

        var technova = await SendForIdAsync(sender, new CreateBrandCommand("TechNova"), cancellationToken);
        var urbanWear = await SendForIdAsync(sender, new CreateBrandCommand("UrbanWear"), cancellationToken);
        var homeCraft = await SendForIdAsync(sender, new CreateBrandCommand("HomeCraft"), cancellationToken);

        // --- Catalog: products + variants + stock -------------------------------
        // Image keywords are matched to each product on purpose (LoremFlickr returns a real Flickr
        // photo for the given keyword, no API key needed) so a reviewer sees an actual headphone
        // photo on the headphones, not an unrelated landscape placeholder.
        var kulaklik = await CreateProductAsync(sender, bilgisayarAksesuar, technova, "Kablosuz Bluetooth Kulaklık",
            "Aktif gürültü engelleme özellikli, 30 saat pil ömürlü kablosuz kulaklık.", "headphones",
            [new VariantSeed("ELK-001", 899.90m, [], 60)], cancellationToken);
        var saat = await CreateProductAsync(sender, bilgisayarAksesuar, technova, "Akıllı Saat",
            "Nabız ve uyku takibi, bildirim aynalama özellikli akıllı saat.", "smartwatch",
            [new VariantSeed("ELK-002", 1499.90m, [], 35)], cancellationToken);
        await CreateProductAsync(sender, bilgisayarAksesuar, technova, "Mekanik Klavye",
            "Hot-swap anahtarlı, RGB aydınlatmalı mekanik oyuncu klavyesi.", "mechanical-keyboard",
            [new VariantSeed("ELK-003", 1299.00m, [], 25)], cancellationToken);
        var mouse = await CreateProductAsync(sender, bilgisayarAksesuar, technova, "Kablosuz Mouse",
            "Sessiz tıklamalı, ergonomik kablosuz mouse.", "computer-mouse",
            [new VariantSeed("ELK-004", 349.90m, [], 80)], cancellationToken);
        await CreateProductAsync(sender, bilgisayarAksesuar, urbanWear, "Laptop Sırt Çantası",
            "Su geçirmez kumaştan, 15.6 inç laptop bölmeli sırt çantası.", "backpack",
            [new VariantSeed("AKS-001", 799.00m, [], 40)], cancellationToken);
        await CreateProductAsync(sender, sporOutdoor, urbanWear, "Erkek Spor Ayakkabı",
            "Nefes alabilir file üst yüzeyli, hafif taban spor ayakkabı.", "sneakers",
            [
                new VariantSeed("SPR-001-40", 1199.00m, [(beden, "40")], 20),
                new VariantSeed("SPR-001-42", 1199.00m, [(beden, "42")], 30),
                new VariantSeed("SPR-001-44", 1199.00m, [(beden, "44")], 18),
            ], cancellationToken);
        var tisort = await CreateProductAsync(sender, giyim, urbanWear, "Kadın Basic Tişört",
            "Pamuklu, regular kesim basic tişört.", "t-shirt",
            [
                new VariantSeed("GYM-001-WHT", 249.90m, [(renk, "Beyaz")], 50),
                new VariantSeed("GYM-001-BLK", 249.90m, [(renk, "Siyah")], 50),
            ], cancellationToken);
        await CreateProductAsync(sender, giyim, urbanWear, "Erkek Kot Pantolon",
            "Slim fit, esnek kumaşlı kot pantolon.", "jeans",
            [
                new VariantSeed("GYM-002-30", 599.00m, [(beden, "30")], 22),
                new VariantSeed("GYM-002-32", 599.00m, [(beden, "32")], 28),
                new VariantSeed("GYM-002-34", 599.00m, [(beden, "34")], 20),
            ], cancellationToken);
        await CreateProductAsync(sender, evYasam, homeCraft, "Kahve Makinesi",
            "Otomatik öğütücülü, tam otomatik espresso makinesi.", "coffee-maker",
            [new VariantSeed("EV-001", 2199.00m, [], 15)], cancellationToken);
        await CreateProductAsync(sender, sporOutdoor, homeCraft, "Yoga Matı",
            "Kaymaz yüzeyli, 6mm kalınlığında yoga matı.", "yoga-mat",
            [
                new VariantSeed("SPR-002-PUR", 349.00m, [(renk, "Mor")], 45),
                new VariantSeed("SPR-002-GRY", 349.00m, [(renk, "Gri")], 45),
            ], cancellationToken);
        await CreateProductAsync(sender, evYasam, homeCraft, "Robot Süpürge",
            "Haritalama özellikli, uygulamadan kontrol edilebilen robot süpürge.", "robot-vacuum",
            [new VariantSeed("EV-002", 5499.00m, [], 12)], cancellationToken);

        // --- Promotion: kuponlar --------------------------------------------------
        var now = DateTime.UtcNow;
        await SendForIdAsync(sender, new CreateCouponCommand("HOSGELDIN10", CouponDiscountType.Percentage, 10m, now.AddDays(-1), now.AddYears(1), null), cancellationToken);
        await SendForIdAsync(sender, new CreateCouponCommand("SEPET100", CouponDiscountType.FixedAmount, 100m, now.AddDays(-1), now.AddYears(1), 100), cancellationToken);

        // --- Identity: demo müşteriler + adresleri ---------------------------------
        var customer1 = await SendForIdAsync(sender, new RegisterUserCommand("ayse.demo@example.com", DemoPassword, "Ayşe", "Yılmaz", "5551112233"), cancellationToken);
        var customer1Address = await SendForIdAsync(sender, new AddAddressCommand(customer1, "Ev", "Ayşe Yılmaz", "5551112233", "İstanbul", "Kadıköy", "Caferağa Mah. Moda Cad. No:12 D:4", "34710", true), cancellationToken);

        var customer2 = await SendForIdAsync(sender, new RegisterUserCommand("mehmet.demo@example.com", DemoPassword, "Mehmet", "Kaya", "5552223344"), cancellationToken);
        var customer2Address = await SendForIdAsync(sender, new AddAddressCommand(customer2, "Ev", "Mehmet Kaya", "5552223344", "Ankara", "Çankaya", "Kızılay Mah. Atatürk Bulvarı No:45 D:8", "06420", true), cancellationToken);

        var customer3 = await SendForIdAsync(sender, new RegisterUserCommand("zeynep.demo@example.com", DemoPassword, "Zeynep", "Demir", "5553334455"), cancellationToken);

        // --- Cart + Order + Shipping + Review ---------------------------------------
        // Müşteri 1: uçtan uca tamamlanmış — teslim edildi + onaylı yorum, en dolu senaryo.
        var order1 = await CreateOrderAsync(sender, customer1, customer1Address, [(kulaklik.Variants[0], 1)], "HOSGELDIN10", cancellationToken);
        await SendAsync(sender, new MarkOrderAsPaidCommand(order1), cancellationToken);
        await SendAsync(sender, new MarkOrderAsPreparingCommand(order1), cancellationToken);
        var shipment1 = await SendForIdAsync(sender, new CreateShipmentCommand(order1, "Yurtiçi Kargo", "YK-DEMO-0001"), cancellationToken);
        await SendAsync(sender, new MarkShipmentDeliveredCommand(shipment1), cancellationToken);
        var review1 = await SendForIdAsync(sender, new CreateReviewCommand(customer1, kulaklik.ProductId, order1, 5, "Ses kalitesi harika, pil ömrü de tam açıklandığı gibi. Kesinlikle tavsiye ederim."), cancellationToken);
        await SendAsync(sender, new ApproveReviewCommand(review1), cancellationToken);

        // Müşteri 2: kargoda — "Siparişlerim" sayfasında canlı bir takip senaryosu.
        var order2 = await CreateOrderAsync(sender, customer2, customer2Address, [(saat.Variants[0], 1), (mouse.Variants[0], 2)], null, cancellationToken);
        await SendAsync(sender, new MarkOrderAsPaidCommand(order2), cancellationToken);
        await SendAsync(sender, new MarkOrderAsPreparingCommand(order2), cancellationToken);
        await SendForIdAsync(sender, new CreateShipmentCommand(order2, "Aras Kargo", "AR-DEMO-0002"), cancellationToken);

        // Müşteri 3: sepette ürün var ama sipariş verilmedi — canlı checkout denemesi için.
        var cartId3 = await SendForIdAsync(sender, new GetOrCreateCartForUserCommand(customer3), cancellationToken);
        await SendAsync(sender, new AddItemToCartCommand(cartId3, tisort.Variants[0], 2), cancellationToken);
    }

    private sealed record SeededProduct(Guid ProductId, IReadOnlyList<Guid> Variants);

    private sealed record VariantSeed(string SkuSuffix, decimal Price, IReadOnlyList<(Guid AttributeId, string Value)> AttributeValues, int StockQuantity);

    private static async Task<SeededProduct> CreateProductAsync(
        ISender sender,
        Guid categoryId,
        Guid brandId,
        string name,
        string description,
        string imageKeyword,
        IReadOnlyList<VariantSeed> variantSeeds,
        CancellationToken cancellationToken)
    {
        var productId = await SendForIdAsync(sender, new CreateProductCommand(categoryId, brandId, name, description), cancellationToken);

        await SendForIdAsync(sender, new AddProductImageCommand(productId, $"https://loremflickr.com/640/480/{imageKeyword}", true), cancellationToken);

        var variants = new List<Guid>();

        foreach (var variantSeed in variantSeeds)
        {
            var attributeValues = variantSeed.AttributeValues
                .Select(av => new ProductVariantAttributeValueInput(av.AttributeId, av.Value))
                .ToList();

            var variantId = await SendForIdAsync(
                sender,
                new AddProductVariantCommand(productId, variantSeed.SkuSuffix, variantSeed.Price, attributeValues),
                cancellationToken);

            // AddProductVariantCommand raises ProductVariantCreatedDomainEvent, which only reaches
            // Inventory asynchronously through the outbox relay (bkz. Program.cs'in
            // app.StartAsync()/WaitForShutdownAsync() kullanma sebebi) — IncreaseStockCommand'ı
            // çağırmadan önce StockItem'ın gerçekten oluştuğunu bekliyoruz.
            await WaitForStockItemAsync(sender, variantId, cancellationToken);
            await SendAsync(sender, new IncreaseStockCommand(variantId, variantSeed.StockQuantity, "Demo seed"), cancellationToken);

            variants.Add(variantId);
        }

        return new SeededProduct(productId, variants);
    }

    private static async Task<Guid> CreateOrderAsync(
        ISender sender,
        Guid userId,
        Guid addressId,
        IReadOnlyList<(Guid VariantId, int Quantity)> items,
        string? couponCode,
        CancellationToken cancellationToken)
    {
        var cartId = await SendForIdAsync(sender, new GetOrCreateCartForUserCommand(userId), cancellationToken);

        foreach (var (variantId, quantity) in items)
        {
            await SendAsync(sender, new AddItemToCartCommand(cartId, variantId, quantity), cancellationToken);
        }

        return await SendForIdAsync(sender, new CreateOrderCommand(userId, addressId, couponCode), cancellationToken);
    }

    private static async Task WaitForStockItemAsync(ISender sender, Guid variantId, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(20);

        while (true)
        {
            var result = await sender.Send(new GetStockByVariantIdQuery(variantId), cancellationToken);

            if (result.IsSuccess)
            {
                return;
            }

            if (DateTime.UtcNow > deadline)
            {
                throw new InvalidOperationException(
                    $"Timed out waiting for the outbox relay to create a stock item for variant {variantId}.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        }
    }

    private static async Task<Guid> SendForIdAsync(ISender sender, ICommand<Guid> command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"Demo data seeding failed for {command.GetType().Name}: {result.Error.Code} - {result.Error.Message}");
    }

    private static async Task SendAsync(ISender sender, ICommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Demo data seeding failed for {command.GetType().Name}: {result.Error.Code} - {result.Error.Message}");
        }
    }
}
