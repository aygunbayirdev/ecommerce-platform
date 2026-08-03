# E-Ticaret Platformu — Görev Listesi

Bkz. [CLAUDE.md](./CLAUDE.md) mimari kararlar ve teknik konvansiyonlar için. Her görev için dev loop: Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → burada işaretle.

Görevler **MVP'ye ulaşana kadar yapılacak her şeyi** kapsayan fazlara ayrılmıştır. Fazlar sırayla yapılmak üzere tasarlandı (her faz bir öncekine bağımlı), ama bir fazın içindeki maddeler arasında da bağımlılık sırası var — madde sırasını değiştirmeden ilerlemek önerilir.

---

## Faz 1 — İskelet & Kimlik ✅ tamamlandı

- [x] **Solution iskeleti + Identity modülü (walking skeleton)** — Clean Architecture, modüler monolith, modül başına şema, CQRS (MediatR + EF Core yazma + Dapper okuma), outbox pattern. 9 modülün proje iskeleti kuruldu (derlenir, DI'a kayıtlı); Identity modülü uçtan uca tam (kayıt, giriş, JWT access+refresh token, GET /api/users/{id}). Commit: `5c5d0d2`. Repo: https://github.com/aygunbayirdev/ecommerce-platform

---

## Faz 2 — Çekirdek Alışveriş Döngüsü (MVP'nin omurgası)

Amaç: bu faz bitince bir müşteri uçtan uca alışveriş yapabilmeli (ürüne bak → sepete ekle → sipariş ver → öde). Sıralama bağımlılığa göre: Identity (adres) → Catalog (ne satılıyor) → Inventory (stok var mı) → Cart (sepete ekle) → Order (sipariş) → Payment (öde).

- [x] **Identity: Adres Defteri** — `Address`, `User` aggregate'inin child entity'si olarak modellendi (mutasyon metodları `internal`, sadece `User` üzerinden erişilebilir — "en fazla bir varsayılan adres" kuralı böylece aggregate root seviyesinde garanti altına alındı). 5 command/query (`AddAddressCommand`, `UpdateAddressCommand`, `DeleteAddressCommand`, `SetDefaultAddressCommand`, `GetAddressesByUserIdQuery`) + `AddressesController` (`/api/addresses`, UserId JWT claim'inden okunur). Yol boyunca iki bug bulunup düzeltildi: (1) EF Core, `Guid.CreateVersion7()` ile constructor'da üretilen id'leri olan yeni child entity'leri "Modified" sanıyordu (bkz. `ModelBuilderExtensions.ApplyClientGeneratedKeys`, artık tüm modüllerde uygulanıyor), (2) boş iskelet modüllerin `OutboxProcessor`'ı henüz var olmayan tabloları sorgulayıp sürekli hata veriyordu (ilgili modül gerçek entity'ler alana kadar kayıt kaldırıldı). 12/12 test geçiyor.
- [x] **Catalog modülü — referans veriler** — `Category` (self-referencing hiyerarşi + `CategoryAttributes` child collection), `Brand`, `ProductAttribute` (global tanım seti). `Category.AssignAttribute`/`RemoveAttribute` "aynı özellik iki kez atanamaz" kuralını aggregate root seviyesinde garanti eder (unique index DB'de de var). 11 command/query + `CategoriesController`/`BrandsController`/`ProductAttributesController` — GET endpoint'leri anonim (public katalog gezinme), POST/PUT/attribute-assign `[Authorize(Roles="Admin")]` (projede ilk kez rol bazlı yetkilendirme kullanıldı). 6/6 test geçiyor. Not: kullanıcıyı Admin yapan bir komut henüz yok, test için DB'de elle güncellendi.
- [x] **Catalog modülü — Product/Variant/Image** — `Product` aggregate root, `ProductVariant`/`ProductImage` child'ları, `ProductVariantAttributeValue` de `ProductVariant`'ın child'ı (3 seviyeli hiyerarşi, projede ilk kez). `AddProductVariantCommand` handler'ı her `ProductAttributeId`'nin ürünün kategorisine atanmış olduğunu doğruluyor (cross-aggregate kontrol) + SKU global unique. `ProductImage.AddImage` ilk görseli veya `isPrimary:true` isteneni otomatik primary yapıyor, öncekini unmark ediyor. Okuma tarafında ilk kez `PagedResult<T>` kullanıldı (`GetProductsByCategoryQuery`, Dapper `QueryMultipleAsync` ile count+page tek round-trip) ve çok tablolu Dapper composition (`GetProductByIdQuery` — product+variants+attribute değerleri+images, 4 result set). 5 command/query + `ProductsController` (GET'ler anonim, POST'lar Admin-only). Yol boyunca bir bug bulunup düzeltildi: Dapper'ın record materyalizasyonu SQL `SELECT` kolon sırasına göre pozisyonel eşleşiyor, DTO'nun constructor parametre sırasıyla SQL'in kolon sırası uyuşmayınca `InvalidOperationException` fırlatıyordu — SQL kolon sırası DTO ile hizalanarak düzeltildi. 14/14 test geçiyor. **Domain event kararı:** `ProductVariantCreatedDomainEvent` gibi bir event (Inventory'nin otomatik `StockItem` açması için) bilinçli olarak eklenmedi — henüz tüketecek bir handler yok, Inventory görevine gelindiğinde event+tüketici birlikte eklenip uçtan uca test edilecek.
- [x] **Inventory modülü** — `StockItem` (aggregate root: ProductVariantId unique, AvailableQuantity, ReservedQuantity) + `StockMovement` (child, audit log — şimdilik sadece `Inbound` tipi, `Reserved`/`Released`/`Committed` Order görevinde eklenecek). `CreateStockItemCommand` bilinçli olarak **idempotent** (var olan variant için Conflict değil, mevcut Id'yi döner — outbox'ın at-least-once teslimat garantisiyle uyumlu olsun diye). **Projenin ilk gerçek modüller arası domain event akışı burada devreye girdi:** `Catalog`, `ProductVariant.Create`'de `ProductVariantCreatedDomainEvent` raise ediyor (outbox artık Catalog'a da bağlı — `OutboxMessageConfiguration` `BuildingBlocks.Infrastructure`'a taşınarak paylaşımlı hale getirildi), `Catalog.Application`'daki `ProductVariantCreatedDomainEventHandler` (WMS deseni: kaynak modül kendi handler'ında hedef modülün command'ini çağırır) `Inventory.Application`'a proje referansıyla `CreateStockItemCommand`'ı gönderiyor. Uçtan uca doğrulandı: varyant oluştur → birkaç saniye içinde `inventory.stock_items`'da otomatik satır açılıyor. `StockItemsController` (`GET`/`POST .../increase`, ikisi de Admin-only — stok miktarı müşteriye açık değil). `Reserve`/`Release`/`Commit` rezervasyon metodları bilinçli olarak bu turda yok, Order modülü görevinde eklenecek. Commit öncesi modüller arası event zincirini ve iş kuralı bütünlüğünü denetleyen bir tur yapıldı — event tetikleme mekanizması eksiksiz bulundu, ayrıca `RemoveCategoryAttributeCommand`'ın bir kategorideki varyantlarca kullanılan bir attribute'un kaldırılmasına izin verdiği bir referential-integrity boşluğu bulunup düzeltildi (`IProductWriteRepository.IsAttributeUsedByAnyVariantInCategoryAsync` ile Conflict döner artık). 34/34 test geçiyor (Identity+Catalog+Inventory toplamı).
- [x] **Cart modülü** — `Cart` (aggregate root: UserId nullable — guest sepeti) + `CartItem` (child, ProductVariantId+Quantity, fiyat/isim **saklanmaz**). İki tasarım kararı netleşti: (1) **Misafir sepeti adresleme** — misafir için sepet kendi `CartId`'siyle adreslenir (`POST /api/carts` anonim yeni sepet açar, client id'yi saklar), giriş yapmış kullanıcı için sepet `UserId`'den otomatik resolve/oluşturulur (`GetOrCreateCartForUserCommand`, JWT claim, route'ta hiç görünmez — Identity `AddressesController`'ın `CurrentUserId` deseninin aynısı). Misafir sepetinin girişte kullanıcı sepetiyle birleşmesi ("merge on login") bilinçli olarak kapsam dışı bırakıldı. (2) **Projenin ilk cross-module READ composition'ı** — `CartItem` fiyat/isim saklamadığı için (sepette fiyat hep canlı olmalı, Order'ın snapshot ilkesiyle simetrik ama ters yönde), `GetCartByIdQueryHandler` ve `AddItemToCartCommandHandler` Catalog'un yeni `GetProductVariantSummariesQuery`'sini **doğrudan senkron `ISender.Send()` çağrısıyla** kullanıyor — Catalog→Inventory'deki event+outbox deseninden bilinçli farkı: bu bir okuma kompozisyonu ve immediate consistency gerektiriyor, event'in kabul ettiği eventual consistency burada uygun değil. `Cart.Application`, yeni bir proje referansıyla `Catalog.Application`'a bağlandı. `CartsController`'da guest uçları (`/api/carts`, `/api/carts/{cartId}/...`) anonim, kullanıcı uçları (`/api/carts/mine/...`) `[Authorize]`. Uçtan uca doğrulandı: guest akışı (oluştur→ekle→miktar güncelle→sil→geçersiz varyant 404), kullanıcı akışı (`/mine` ilk çağrıda otomatik boş sepet açıyor, sonraki çağrılarda aynı sepeti döndürüyor, token'sız 401). 10/10 yeni test (Cart) + 44/44 toplam test geçiyor.
- [ ] **Order modülü** — Tablolar: `Orders` (OrderNumber, Status, adres **snapshot**'ı — Identity.Addresses'e FK değil, o anki değerin kopyası), `OrderItems` (ürün adı + fiyat **snapshot**'ı — Catalog'daki fiyat sonradan değişse bile geçmiş sipariş bozulmaz), `OrderStatusHistory` (state machine: Created → PaymentPending → Paid → Preparing → Shipped → Delivered/Cancelled).
- [ ] **Payment modülü (monolith içinde ilk versiyon)** — Tablolar: `Payments` (OrderId, Amount, Status, Method), `PaymentTransactions` (gateway deneme kayıtları, idempotency key). Bu aşamada hâlâ monolith'in içinde, diğer modüllerden farksız — mock/test ödeme akışı (gerçek gateway entegrasyonu yok). Domain event ile Order → Payment akışı hâlâ **in-process MediatR** üzerinden (RabbitMQ henüz yok, bkz. Faz 4).

**Faz 2 bitiş kriteri:** bir müşteri register olup, ürünlere bakıp, sepete ekleyip, adres seçip sipariş verip, (mock) ödeme yapabiliyor olmalı — uçtan uca API testiyle doğrulanmalı (Identity Faz 1'de yapıldığı gibi curl/Postman ile).

---

## Faz 3 — Deneyim Zenginleştirme

Temel akış Faz 2 sonunda çalışıyor olacak; bu faz gerçekçiliği ve Trendyol-benzeri deneyimi tamamlıyor. Modüller arasında sıkı bağımlılık yok, sıra önemsiz.

- [ ] **Promotion modülü** — Tablolar: `Coupons` (Code, DiscountType, DiscountValue, ValidFrom/To, UsageLimit), `CouponRedemptions`.
- [ ] **Review modülü** — Tablolar: `Reviews` (ProductId, UserId, **OrderId** — sadece satın alanlar yorum yapabilsin diye doğrulama, Rating, Comment, IsApproved).
- [ ] **Shipping modülü** — Tablolar: `Shipments` (OrderId, Carrier, TrackingNumber, Status), `ShipmentStatusHistory`.

---

## Faz 4 — Payment'ı Mikroserviste Çıkar (Strangler Fig)

Projenin asıl öğrenme amacı — sağlam bir monolith olmadan (Faz 2-3 tamamlanmadan) anlamsız olduğu için burada. Bkz. CLAUDE.md madde 2 ve 4 (Domain Event vs. Entegrasyon Eventi ayrımı).

- [ ] **RabbitMQ'yu docker-compose'a ekle** — şimdiye kadar hiç yoktu (in-process MediatR yeterliydi).
- [ ] **Order → Payment entegrasyon eventi** — Order modülü bir entegrasyon eventi yayınlar (RabbitMQ üzerinden), Payment bunu tüketir. WMS'teki outbox+domain event akışına benzer ama artık process sınırını aşıyor.
- [ ] **Payment'ı ayrı deploy birimine çıkar** — kendi repo'su (ya da en azından bu repo içinde ayrı, bağımsız derlenen/deploy edilen bir proje), kendi Postgres veritabanı (mevcut monolith DB'sinden tamamen ayrı — bkz. "Payment teknoloji seçimi" açık sorusu).
- [ ] **Monolith'ten Payment modülünü kaldır** — `src/Modules/Payment` monolith solution'dan çıkar, yerine sadece entegrasyon eventi gönderen ince bir client/adapter kalır.
- [ ] **Uçtan uca doğrulama** — sipariş verildiğinde monolith'in RabbitMQ'ya event yolladığı, ayrı Payment servisinin bunu işleyip kendi DB'sine yazdığı, eventual consistency'nin gözlemlenebildiği gösterilmeli (loglar/DB sorgusuyla).

---

## Faz 5 — Frontend

Henüz hiçbir karar verilmedi (bkz. CLAUDE.md "Henüz Karar Verilmemiş"). Bu fazın ilk maddesi bir sonraki konuşmada karar verilecek açık sorular içeriyor.

- [ ] **Frontend teknoloji kararları** — public site vs admin panel tek proje mi ayrı mı, UI kütüphanesi seçimi (shadcn/ui vs public tarafta farklı bir kütüphane).
- [ ] **Public site** — ürün listeleme, ürün detay, sepet, checkout, sipariş takibi, (Faz 3 sonrası) yorum/puanlama, kupon uygulama.
- [ ] **Admin panel** — katalog yönetimi, sipariş yönetimi, stok yönetimi, kampanya yönetimi.

---

## Faz 6 — Üretime Hazırlık / Cilalama

- [ ] **CI/CD** — build + test pipeline (GitHub Actions), WMS'te de önerilip başlanmamıştı, bu projede en azından PR başına build+test çalıştırılması hedefleniyor.
- [ ] **Test kapsamını genişlet** — her modül için Identity'dekine benzer unit test kapsamı (handler testleri), en az Order ve Payment için entegrasyon testleri (Testcontainers ile gerçek Postgres).
- [ ] **Deploy stratejisi netleştir** — Docker Compose'un yeterli olup olmadığına karar ver (Kubernetes şimdilik "gereksiz rabbit hole" olarak değerlendirildi, kesin karar yok).

---

## Notlar

- Bir fazın tamamlanması için tüm alt maddelerinin bitmesi şart değil — her madde kendi başına bağımsız bir "görev" (dev loop'un bir turu), tamamlandıkça işaretlenir.
- Faz sırası kesin değil, gerekirse (ör. frontend'i erken görmek istenirse Faz 5 öne çekilebilir) yeniden değerlendirilebilir — ama bir fazın içindeki madde sırası bağımlılık gerektirir.
