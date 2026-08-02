# E-Ticaret Platformu — Görev Listesi

Bkz. [CLAUDE.md](./CLAUDE.md) mimari kararlar için. Her görev için dev loop: Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → burada işaretle.

## Backend

- [x] **Solution iskeleti + Identity modülü (walking skeleton)** — Clean Architecture, modüler monolith, modül başına şema, CQRS (MediatR + EF Core yazma + Dapper okuma), outbox pattern. 9 modülün proje iskeleti kuruldu (derlenir, DI'a kayıtlı); Identity modülü uçtan uca tam (kayıt, giriş, JWT access+refresh token, GET /api/users/{id}).
- [ ] **Identity: Adres defteri** — Address CRUD command/query'leri (AddAddressCommand, UpdateAddressCommand, DeleteAddressCommand, GetAddressesByUserIdQuery, SetDefaultAddressCommand).
- [ ] **Catalog modülü** — Category (hiyerarşik), Brand, Product, ProductVariant, dinamik varyant özellik modeli (ProductAttributes, CategoryAttributes, ProductVariantAttributeValues), ProductImages.
- [ ] **Inventory modülü** — StockItems, StockMovements; sipariş oluşturulunca rezervasyon (sepette stok kilitlenmez, MVP kararı).
- [ ] **Cart modülü** — Carts, CartItems (guest + kullanıcı sepeti).
- [ ] **Order modülü** — Orders, OrderItems (ürün adı/fiyat snapshot), OrderStatusHistory (state machine).
- [ ] **Promotion modülü** — Coupons, CouponRedemptions.
- [ ] **Review modülü** — Reviews (satın alma doğrulamalı, OrderId ile).
- [ ] **Shipping modülü** — Shipments, ShipmentStatusHistory.
- [ ] **Payment modülü (monolith içinde ilk versiyon)** — Payments, PaymentTransactions; mock/test ödeme akışı.
- [ ] **Payment'ı mikroservise çıkar (Strangler Fig)** — ayrı repo/deploy birimi, kendi Postgres DB'si, RabbitMQ üzerinden entegrasyon eventi (Order → Payment), monolith'ten koparma.

## Frontend

- [ ] **Frontend teknoloji kararları** — public site vs admin panel tek proje mi ayrı mı, UI kütüphanesi seçimi (shadcn/ui vs alternatif).
- [ ] **Public site** — ürün listeleme, ürün detay, sepet, checkout, sipariş takibi.
- [ ] **Admin panel** — katalog yönetimi, sipariş yönetimi, stok yönetimi, kampanya yönetimi.

## Altyapı

- [ ] **CI/CD** — henüz başlanmadı, kapsam netleşince eklenecek.
- [ ] **Test stratejisi** — modül bazlı unit test kapsamı genişletilecek, entegrasyon testleri (Testcontainers ile Postgres) eklenecek.
