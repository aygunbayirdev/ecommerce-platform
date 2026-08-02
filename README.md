# E-Ticaret Platformu

Modüler monolith olarak kurulan, ileride tek bir modülün (Payment) gerçek bir mikroservise çıkarılacağı bir e-ticaret backend'i. Mimari kararların gerekçesi için [CLAUDE.md](./CLAUDE.md), görev takibi için [TASKS.md](./TASKS.md).

## Mimari

- **.NET 10 / ASP.NET Core**, Clean Architecture (Domain → Application → Infrastructure → Api)
- **Modüler monolith**: her modül kendi PostgreSQL şemasına sahip (`identity`, `catalog`, `inventory`, `cart`, `order`, `payment`, `shipping`, `promotion`, `review`)
- **CQRS**: MediatR ile in-process command/query dispatch; komutlar EF Core (yazma), sorgular Dapper (okuma) kullanır
- **Outbox pattern**: domain event'ler aynı transaction içinde `outbox_messages` tablosuna yazılır, arka planda bir `BackgroundService` bunları MediatR üzerinden işler
- **JWT (access + refresh token)** ile kimlik doğrulama

Şu an sadece **Identity** modülü uçtan uca tam (kayıt, giriş, token yenileme, kullanıcı sorgulama). Diğer 8 modül (Catalog, Inventory, Cart, Order, Payment, Shipping, Promotion, Review) proje iskeleti olarak kurulu — DI'a kayıtlı, derleniyor, ama henüz domain kodu yok. Detay için [TASKS.md](./TASKS.md).

## Çalıştırma

```bash
cp .env.example .env   # gerekirse değerleri düzenle
docker compose up -d postgres
```

```bash
cd backend
dotnet ef database update \
  --project src/Modules/Identity/ECommercePlatform.Modules.Identity.Infrastructure \
  --startup-project src/ECommercePlatform.Api

dotnet run --project src/ECommercePlatform.Api
```

API `https://localhost:5001` (veya `ASPNETCORE_URLS` ile belirtilen adreste) ayağa kalkar, Development ortamında `/openapi/v1.json` üzerinden OpenAPI şeması erişilebilir.

### Docker ile tam yığın

```bash
docker compose up -d
```

## Test

```bash
cd backend
dotnet test tests/ECommercePlatform.Modules.Identity.UnitTests
```

## Bilinen durum

MediatR 14, v13'ten itibaren ticari lisans gerektiriyor (dev/test kullanımı ücretsiz — bkz. log'lardaki `LuckyPennySoftware.MediatR.License` uyarısı). Bu proje kapsamında bilinçli olarak kullanılmaya devam ediliyor; production'a alınırsa lisans veya ücretsiz bir alternatif (ör. [Mediator](https://github.com/martinothamar/Mediator)) değerlendirilmeli.
