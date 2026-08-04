# E-Ticaret Platformu — Proje Notları

Bu dosya projenin tüm mimari kararlarını, gerekçelerini ve kod yazarken uyulacak teknik konvansiyonları içerir. Amaç: bu konuşma geçmişi silinse veya haftalar/aylar sonra yeni bir konuşmada devam edilse bile, hiçbir bağlam kaybı olmadan kaldığı yerden devam edilebilmesi.

**Güncel durum ve sıradaki iş için:** [TASKS.md](./TASKS.md) — fazlara ayrılmış, MVP'ye kadar olan tüm iş listesi orada. Bu dosya (CLAUDE.md) *neden*i, TASKS.md *ne*yi ve *sırayı* anlatır.

**Repo:** https://github.com/aygunbayirdev/ecommerce-platform (public)

## Şu An Neredeyiz

**Faz 2 tamamlandı** — Identity adres defteri, Catalog (referans veriler + Product/Variant/Image), Inventory (+ projenin ilk modüller arası domain event akışı: Catalog→Inventory), Cart (+ projenin ilk cross-module READ composition'ı: Cart→Catalog senkron `ISender` query çağrısı), Order (+ projenin ilk çok-modüllü checkout orkestrasyonu: Order→Cart/Identity/Inventory, dağıtık transaction yerine sıralamayla çözülen atomiklik) ve **Payment modülü** (mock/test ödeme akışı — bkz. madde 9 aşağıda, gerçek gateway Stripe denendi ama Türkiye desteklenmediği için vazgeçildi, iyzico'ya Faz 7'de geçilecek) tamamlandı. Bir müşteri artık register olup, ürünlere bakıp, sepete ekleyip, adres seçip sipariş verip, (mock) ödeme yapabiliyor — Faz 2 bitiş kriteri karşılandı.

**Faz 3'e başlandı** — **Promotion modülü** (kupon/indirim) tamamlandı: `Coupon`/`CouponRedemption`, checkout orkestrasyonuna (`CreateOrderCommandHandler`) stok rezervasyonundan sonraki üçüncü adım olarak eklendi (Inventory'nin Reserve/Release deseninin kupon kullanımına uygulanışı — bkz. TASKS.md Faz 3 detayı). Sıradaki iş Review ve Shipping (sıra önemsiz). Detay ve tam sıralama için [TASKS.md](./TASKS.md).

## Geliştirme Döngüsü

Her görev için: **Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → `TASKS.md`'de işaretle.**

Kurallar:
- **Commit mesajları her zaman İngilizce yazılır** (proje dili Türkçe olsa da).
- Commit mesajlarına `Co-Authored-By` gibi Claude/Anthropic referansı eklenmez.
- Büyük/çok dosyalı işler (yeni modül gibi) önce plan olarak sunulur, onay alındıktan sonra koda geçilir.

## Amaç ve Bağlam

- Modüler monolith / Clean Architecture gibi kavramlar WMS projesinde uygulandı (bkz. `WarehouseManagementSystem/CLAUDE.md`, yerel yol: `C:\Users\turko\OneDrive\Desktop\WarehouseManagementSystem`) — bu proje kapsamında mikroservis mimarisi yeni öğreniliyor.
- Portföy projelerinin "gösteriş için yapılmış" değil, **gerekçeli ve savunulabilir** mimari kararlar içermesi önemli görülüyor.
- Bu proje, WMS'in (modüler monolith) tamamlayıcısı olarak tasarlandı: WMS "iyi yapılmış bir monolith" hikâyesini anlatıyor, bu proje "mikroservise ne zaman/neden geçilir" hikâyesini anlatacak. İkisi birlikte "hangi mimariyi ne zaman seçeceğini biliyorum" anlatısını kuruyor.
- Kullanıcı mikroservis mimarisini **hiç bilmiyordu**, öğreniyor. İki kriteri var:
  1. **Hazmedilebilirlik**: Seviyesine göre, kafası karışmadan öğrenmek istiyor.
  2. **Savunulabilirlik**: Bir işveren projeye baktığında "bu adam saçmalamış" dememeli — mimari kararların gerçek bir gerekçesi olmalı.

## Alınan Mimari Kararlar

### 1. Domain: E-Ticaret

Bankacılık değil e-ticaret seçildi çünkü bankacılık domaini gereksiz regülasyon/compliance karmaşıklığı ekliyor ve asıl öğrenilmek istenen şeyin (mikroservis mimarisi) üzerini örtüyor. E-ticaret hem daha standart/iyi belgelenmiş bir domain hem de mikroservis için doğal servis sınırları sunuyor.

### 2. Yaklaşım: Modüler Monolith + Tek Servis Çıkarma (Strangler Fig)

**Sıfırdan çoklu mikroservis mimarisi KURULMAYACAK.** Bunun yerine:

1. Proje önce WMS'teki gibi bir **modüler monolith** olarak kuruldu (Clean Architecture, modül başına şema, CQRS — WMS'teki kanıtlanmış desen tekrar kullanıldı).
2. Sonra **Payment modülü** (bkz. madde 3), gerçek, ayrı bir mikroservise çıkarılacak: ayrı deploy birimi, kendi veritabanı, monolith'le mesaj kuyruğu üzerinden asenkron haberleşen bağımsız bir servis (bkz. TASKS.md Faz 4).

Bu yaklaşım **Martin Fowler'ın Strangler Fig pattern'i** ve Sam Newman'ın "Monolith to Microservices" kitabının temel yaklaşımıdır — gerçek şirketlerin mikroservise geçiş şekli budur, "trend diye mikroservis yapmak" değildir. Bu, mülakatta "neden mikroservis" sorusuna savunulabilir bir cevap verir.

**Neden bu, hazmedilebilir?** Sıfırdan 4-5 servis + API gateway + service discovery + distributed tracing + saga orchestration hepsi birden öğrenilseydi bu seviye için çok fazla olurdu. Tek servis çıkarımıyla öğrenilecek yeni kavram seti küçük ve net: mesaj kuyruğu temelleri, servisler arası asenkron iletişim, eventual consistency, servis-özel veritabanı, bağımsız deploy.

### 3. Ayrılacak Servis: Ödeme (Payment) — kesinleşti

**Ödeme (Payment)** modülü ayrılacak servis olarak kesinleşti (TASKS.md Faz 4). Gerekçe: gerçek sistemlerde ödeme genelde PCI-DSS izolasyonu ve farklı deploy/ölçeklenme ihtiyaçları yüzünden ayrı tutulur — bu, "neden bu servisi ayırdın" sorusuna gerçek bir cevap verir. Trivial bir servis (ör. "email/bildirim gönder") ayırmak yapay/zorlama dururdu, bilinçli olarak kaçınıldı.

Payment, Faz 2'de **önce monolith'in içinde** normal bir modül olarak inşa edilecek (diğer modüllerden farksız), Faz 4'te strangler fig ile koparılacak.

### 4. Domain Event vs. Entegrasyon Eventi — net ayrım

- **Domain event** (monolith'in kendi modülleri arası, aynı process içinde — Catalog/Order/Inventory vb.): **MediatR** ile in-process dispatch edilir, outbox pattern ile güvenceye alınır (aynı process/transaction sınırı içinde kaldığı için). Identity modülünde bu akış zaten çalışıyor (`UserRegisteredDomainEvent` örneği).
- **Entegrasyon eventi** (monolith → Payment mikroservisi, process/deployment sınırını geçen): **RabbitMQ** üzerinden — Kafka değil, daha basit ve öğrenme eğrisi düşük. MediatR in-process bir mekanizma olduğu için servis sınırını aşamaz, bu yüzden mesaj kuyruğu gerekiyor. RabbitMQ, Payment ayrılana kadar (Faz 4) projeye **hiç eklenmeyecek** — Faz 1-3'te sadece in-process MediatR var, docker-compose'da RabbitMQ servisi yok.

### 5. Öğrenme Stratejisi (kurs vs. yaparak öğrenme)

Kullanıcı Fatih Çakıroğlu'nun ~40-50 saatlik Udemy mikroservis kursunun **tamamını baştan izlemeyecek**:

- Sadece giriş bölümü (mikroservis nedir, ne zaman kullanılır, monolith vs mikroservis tradeoff'ları) + RabbitMQ/mesaj kuyruğu bölümü izlenecek.
- Kursun geri kalanı (API gateway, service discovery, Kubernetes, çoklu servis orchestration) bu proje kapsamında **hemen uygulanmayacağı için** şimdilik atlanacak.
- Payment'ı ayırma aşamasına (Faz 4) gelindiğinde kursun ilgili bölümüne referans olarak dönülecek.
- Gerekçe: kullanıcı WMS'i hiç kurs izlemeden, doğrudan inşa ederek + ihtiyaç oldukça araştırarak öğrendi — kendisi için kanıtlanmış bir öğrenme stili.

### 6. Modül Listesi — kesinleşti (tek satıcı, taktik DDD)

İki temel karar modül/tablo tasarımını netleştirdi:
- **Tek satıcı** (marketplace değil) — Trendyol'un UI/UX'ini andırıyor ama backend'de tek taraflı sipariş/ödeme akışı var. Sipariş sub-order'lara bölünmüyor, split payment yok, Seller modülü yok. Gerekçe: MVP kapsamını makul tutmak, "marketplace complexity'si bilinçli olarak scope dışı bırakıldı" diye savunulabilir.
- **Taktik DDD** — Aggregate root + domain event + repository pattern (WMS'teki desenin devamı). Value Object sadece gerçekten anlamlı yerlerde kullanılır, her yerde zorlanmaz. Event storming gibi stratejik DDD süreçlerine girilmedi (MVP portföy projesi için gereksiz).

**9 modül** (her biri kendi Postgres şeması, `src/Modules/{Module}/` altında):

| Modül | Sorumluluk | Durum |
|---|---|---|
| **Identity** | Kimlik doğrulama, kullanıcı, adres defteri | ✅ Auth tam, adres defteri Faz 2 |
| **Catalog** | Ürün, kategori, marka, dinamik varyant özellikleri | Faz 2 |
| **Inventory** | Stok seviyesi, rezervasyon | Faz 2 |
| **Cart** | Sepet (guest + kullanıcı) | Faz 2 |
| **Order** | Sipariş, sipariş kalemi (snapshot), durum geçmişi | Faz 2 |
| **Payment** | Ödeme (önce monolith içi, sonra mikroservis) | Faz 2 tamamlandı (mock gateway) → Faz 4'te çıkarılır, Faz 7'de iyzico |
| **Promotion** | Kupon/indirim | Faz 3 tamamlandı |
| **Review** | Ürün yorumu/puanlama (satın alma doğrulamalı) | Faz 3 |
| **Shipping** | Kargo takibi | Faz 3 |

Modül bazında tablo isimleri ve gerekçeleri için TASKS.md'deki ilgili faz maddelerine bakılabilir (her modülün tabloları orada özetlendi). Kolon seviyesi detay henüz sadece Identity için koda döküldü; diğer modüller için görev başladığında (Planla adımında) netleştirilecek.

**Not — Order snapshot mantığı (önemli DDD noktası):** `Orders.ShippingAddress` ve `OrderItems`'daki ürün adı/fiyat, `Identity.Addresses`/`Catalog.Products`'a FK ile bağlanmaz — sipariş anındaki değerin **kopyası** olarak saklanır. Sebep: kullanıcı adresini/ürün fiyatını sonradan değiştirse bile geçmiş sipariş etkilenmemeli.

**Not — Inventory rezervasyon kararı:** Sepete ürün eklerken stok rezerve edilmez (TTL'li `StockReservations` tablosu ve background expiry job'ı yok — bilinçli MVP basitleştirmesi). Rezervasyon sadece Order oluşturulunca yapılır, bu yüzden 2 tablo (`StockItems` + `StockMovements`) yeterli.

### 7. Backend Teknoloji Yığını — kesinleşti

WMS ile aynı: **.NET 10**, **PostgreSQL**, **Clean Architecture**, **CQRS + MediatR** (in-process), **EF Core** (yazma tarafı) + **Dapper** (okuma tarafı), **outbox pattern** (domain event güvencesi), **Docker Compose**.

### 8. Kimlik Doğrulama — JWT

Access token + refresh token (stateless). Gerekçe: API'nin public site, admin panel ve ileride ayrılacak Payment servisi gibi birden fazla client/servis tarafından tüketilmesi bekleniyor — stateless JWT bu senaryoya cookie-session'dan daha uygun.

### 9. Payment Modülü — Mock Gateway Deseni, Stripe Denemesi ve iyzico Planı

**Neden Stripe değil:** Payment modülü için önce Stripe test-mode entegrasyonu denendi (kullanıcının isteğiyle). Ancak Stripe kayıt ekranında **Türkiye desteklenen ülkeler arasında yok** — kullanıcı gerçek bir finansal serviste ülke bilgisini yanlış beyan etmek istemediği için (doğru ve bilinçli bir karar) bu yoldan vazgeçildi. Bu, "neden X değil de Y" sorusuna gerçek, doğrulanabilir bir cevap.

**Şu anki durum (Faz 2) — mock ödeme akışı:** `Payment.Application`'daki `IPaymentGateway` arayüzü (Identity'deki `IPasswordHasher`/`ITokenGenerator` ile aynı **port/adapter** deseni) her şeyin bağlandığı soyutlama; `Payment.Domain`, `ProcessPaymentCommandHandler`, controller ve testlerin **hiçbiri** gerçek bir gateway olup olmadığını bilmiyor/umursamıyor. Bugünkü implementasyon `MockPaymentGateway` (`Payment.Infrastructure/Gateways/`): kart numarası `"0000"` ile bitiyorsa reddediyor, aksi halde onaylıyor — gerçek test-mode gateway'lerin (Stripe'ın `4242...`/`...0002` gibi) "belirli numaralar belirli sonuç tetikler" konvansiyonunun bilinçli taklidi.

**İdempotency key:** Her ödeme denemesi çağıran tarafın ürettiği bir `idempotencyKey` taşır (`ProcessPaymentCommand`). `Payment.Attempt` aynı key ile ikinci bir çağrıyı Conflict ile reddeder — gerçek gateway'lerin "network timeout sonrası aynı key ile retry = aynı sonuç, çift çekim yok" garantisinin domain seviyesinde taklidi. Başarısız bir deneme `Payment`'ı `Pending`'de bırakır (yeni bir key ile tekrar denenebilir), sadece başarılı bir deneme `Succeeded`'a geçirir (terminal).

**Payment ↔ Order bağımlılık yönü — döngüsel referans riski nasıl önlendi:** `Payment.Application` → `Order.Application` (tek yönlü). Ödeme başarılı olunca zaten var olan `MarkOrderAsPaidCommand`'ı çağırıyor (Order tarafında yeni kod gerekmedi — bu komut daha önce admin-only stand-in olarak eklenmişti). Mock akışta Order'ın Payment'a bir "ödeme hazırlığı" event'i göndermesine gerek yok (gerçek bir gateway'de "PaymentIntent oluştur" gibi bir ön adım olurdu), bu yüzden ters yönde bir bağımlılığa (`Order.Application` → `Payment.Application`) hiç ihtiyaç yok — böyle bir çift yönlü bağımlılık zaten .NET'te derlenmezdi (circular project reference).

**Ödeme başarılı/iptal olunca stok rezervasyonunun çözülmesi:** Order görevinde eklenen `ReserveStockCommand` sadece rezerve ediyordu; Payment görevi sırasında yapılan denetimde (kullanıcının önceki modüllerde istediği "eventler eksiksiz mi" denetiminin aynısı) **rezervasyonun hiç çözülmediği** bulundu — ödenen siparişlerde stok sonsuza kadar `Reserved` durumunda kalıyordu. Düzeltme: `MarkOrderAsPaidCommandHandler` artık `CommitStockCommand` çağırıyor (rezervasyon kalıcı düşüşe dönüşüyor), `CancelMyOrderCommandHandler` sadece **`PaymentPending` durumundan** iptal edilirken `ReleaseStockCommand` çağırıyor (rezervasyon serbest bırakılıyor) — `Created`'dan otomatik iptal (yetersiz stok) hiç rezervasyon yapmadığı için, `Paid`/`Preparing`'den iptal ise zaten committed stoğu ilgilendirdiği (bir iade/restock akışı gerektirir, bu MVP'de yok) için bu iki durumda release çağrılmıyor.

**iyzico'ya geçiş — Faz 7 (opsiyonel, kullanıcı kendi eliyle yapacak):** Site Faz 6 sonunda mevcut mock akışla yayınlanacak. Kullanıcı müsait olduğunda, **öğrenme amaçlı kendi eliyle**, adım adım rehberlik alarak iyzico'yu entegre edecek (bkz. TASKS.md Faz 7). Değişecek olan **tek şey**: `Payment.Infrastructure/Gateways/`'e yeni bir `IyzicoPaymentGateway : IPaymentGateway` sınıfı + `PaymentModule.cs`'teki tek bir DI kaydı satırı (`MockPaymentGateway` yerine `IyzicoPaymentGateway`). `Payment.Domain`, `Payment.Application`, `PaymentsController`, testler — **hiçbiri değişmiyor**. Bu, port/adapter deseninin tam olarak ne işe yaradığının somut kanıtı.

## Teknik Konvansiyonlar (kod yazarken uyulacak kurallar)

WMS'in backend yapısı (`C:\Users\turko\OneDrive\Desktop\WarehouseManagementSystem\backend`) referans alınarak kuruldu; aşağıdaki konvansiyonlar zaten kodda uygulanmış durumda, yeni modüller de aynı şekilde yazılmalı.

**Solution yapısı:**
```
backend/
├── ECommercePlatform.slnx
├── Directory.Build.props        (TargetFramework net10.0, Nullable enable, ImplicitUsings enable)
├── Directory.Packages.props     (Central Package Management — NuGet versiyonları burada, csproj'larda version yok)
├── src/
│   ├── ECommercePlatform.SharedKernel/                        (BaseEntity, IDomainEvent, Result/Result<T>, Error, Guard)
│   ├── BuildingBlocks/
│   │   ├── ECommercePlatform.BuildingBlocks.Application/       (ICommand, IQuery, handler interface'leri, DomainEventNotification, LoggingBehavior, ValidationBehavior)
│   │   └── ECommercePlatform.BuildingBlocks.Infrastructure/    (Outbox: OutboxMessage/OutboxWritingInterceptor/OutboxProcessor<TDbContext>, ISqlConnectionFactory/NpgsqlConnectionFactory, JwtOptions)
│   ├── Modules/{ModuleName}/
│   │   ├── ECommercePlatform.Modules.{Module}.Domain/           (sadece SharedKernel'e bağımlı)
│   │   ├── ECommercePlatform.Modules.{Module}.Application/      (Domain + BuildingBlocks.Application'a bağımlı; Abstractions/, Dtos/, {Feature}/ klasörleri)
│   │   └── ECommercePlatform.Modules.{Module}.Infrastructure/   (Application + BuildingBlocks.Infrastructure'a bağımlı; Persistence/, Repositories/, {Module}Module.cs)
│   └── ECommercePlatform.Api/                                   (composition root: Program.cs, Controllers/, appsettings.json)
└── tests/
    └── ECommercePlatform.Modules.{Module}.UnitTests/
```

**İsimlendirme:** modül adları tekil (Identity, Catalog...), her katman ayrı csproj, namespace = csproj adı. Command/query/handler/validator her biri ayrı dosya (`CreateXCommand.cs`, `CreateXCommandHandler.cs`, `CreateXCommandValidator.cs`), feature-per-aggregate klasörleri (`Users/`, ileride `Products/` vb.). Primary constructor + `sealed` + file-scoped namespace her yerde. Handler'lar `Result`/`Result<T>` döner, business hatası için exception **fırlatılmaz**.

**CQRS akışı:** Command → `ICommand`/`ICommand<T>` (yazma, EF Core repository) → `ICommandHandler`. Query → `IQuery<T>` (okuma, Dapper raw SQL) → `IQueryHandler`. Her modülün `{Module}Module.cs`'i kendi `AddMediatR`/`AddValidatorsFromAssembly` çağrısını yapar (global tek kayıt değil, modül-scoped).

**Outbox akışı:** Entity domain event raise eder (`BaseEntity.AddDomainEvent`) → `OutboxWritingInterceptor` (EF Core `SaveChangesInterceptor`) aynı transaction'da `outbox_messages` tablosuna yazar → `OutboxProcessor<TDbContext>` (generic `BackgroundService`, her modül için ayrı instance, 5 saniyede bir poll eder) MediatR `IPublisher` ile `DomainEventNotification<T>` yayınlar.

**Veritabanı:** Tek PostgreSQL instance, **modül başına şema** (`identity`, `catalog`, vb. — `{Module}DbContext.Schema` sabiti). Kolon/tablo adları **snake_case** (`EFCore.NamingConventions` paketi, `.UseSnakeCaseNamingConvention()` — manuel `HasColumnName` yazılmıyor). Migration'lar `dotnet ef migrations add ... --project src/Modules/{Module}/...Infrastructure --startup-project src/ECommercePlatform.Api` ile üretiliyor; her modülün kendi `{Module}DbContextFactory` (design-time factory) sınıfı var.

**Şifre hash'leme:** `Microsoft.Extensions.Identity.Core` paketindeki `PasswordHasher<TUser>` (ekstra BCrypt gibi bir paket yok).

**JWT:** `System.IdentityModel.Tokens.Jwt` ile üretiliyor, `JwtOptions` (Issuer/Audience/SigningKey/expiration'lar) `BuildingBlocks.Infrastructure.Security`'de tanımlı, hem Identity'nin token üreticisi hem Api'nin `AddJwtBearer` doğrulaması aynı config section'ı (`Jwt`) okuyor.

**Docker Compose gotcha — ÖNEMLİ:** Postgres container'ının host portu **5433**, standart 5432 değil. Sebep: bu makinede WMS projesinin kendi Postgres container'ı zaten 5432'yi kullanıyor, çakışmayı önlemek için ECommercePlatform 5433'e taşındı. `appsettings.json`, `.env.example` ve `IdentityDbContextFactory`'deki fallback connection string hepsi buna göre ayarlı. Yeni bir makinede (WMS çalışmıyorsa) 5432'ye geri alınabilir ama şu an **5433 doğru port**.

**MediatR lisansı — bilinçli kabul edildi:** MediatR v13+ ("Lucky Penny Software") ticari kullanım için ücretli lisans gerektiriyor; dev/test için ücretsiz (loglarda uyarı görünür, zararsız). Bu proje portföy amaçlı olduğu için **v14 ile devam etme kararı bilinçli olarak alındı** — free alternatif (ör. Mediator by Martin Othamar) veya v12.x'e pinleme seçenekleri değerlendirildi ama reddedildi. Production'a alınırsa bu karar yeniden gözden geçirilmeli.

## Referans

WMS/Depo Yönetim Sistemi (`C:\Users\turko\OneDrive\Desktop\WarehouseManagementSystem`, GitHub: `aygunbayirdev/warehouse-management-system`) aynı mimari felsefeyi paylaşan bir modüler monolith örneği — Clean Architecture, CQRS, outbox pattern, naming standardı bu projenin başlangıç noktasıydı ve büyük ölçüde birebir mirror edildi (bkz. "Teknik Konvansiyonlar"). Yeni bir modül eklerken emin olunmayan bir konvansiyon varsa WMS'teki karşılığına bakılabilir.

## Henüz Karar Verilmemiş — İlgili Faza Gelince Netleştirilecek

- **Frontend yapısı** (TASKS.md Faz 5): public site + admin panel tek proje mi ayrı mı; UI kütüphanesi (WMS'te shadcn/ui kullanıldı, birebir aynısı zorunlu değil — public tarafta marka kimliği için farklı bir kütüphane mantıklı olabilir, admin'de shadcn kalabilir).
- **Payment servisinin teknoloji seçimi** (TASKS.md Faz 4): aynı .NET stack mi, yoksa mikroservisin "farklı teknoloji kullanabilme" avantajını göstermek için bilinçli olarak farklı bir stack mi (polyglot persistence/programming örneği olabilir).
- **Deploy stratejisi** (TASKS.md Faz 6): **Hetzner'deki kendi sunucumuza deploy edilecek** — kesinleşti. Docker Compose ile mi, başka bir yöntemle mi olacağı henüz netleşmedi (Kubernetes şimdilik "gereksiz rabbit hole" olarak değerlendirildi). Faz 6 bitip MVP hazır olunca yayına alınacak.
- **CI/CD detayları** (TASKS.md Faz 6): hangi pipeline aracı, hangi aşamalar.
