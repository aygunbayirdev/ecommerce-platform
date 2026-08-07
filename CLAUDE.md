# E-Ticaret Platformu — Proje Notları

Bu dosya projenin tüm mimari kararlarını, gerekçelerini ve kod yazarken uyulacak teknik konvansiyonları içerir. Amaç: bu konuşma geçmişi silinse veya haftalar/aylar sonra yeni bir konuşmada devam edilse bile, hiçbir bağlam kaybı olmadan kaldığı yerden devam edilebilmesi.

**Güncel durum ve sıradaki iş için:** [TASKS.md](./TASKS.md) — fazlara ayrılmış, MVP'ye kadar olan tüm iş listesi orada. Bu dosya (CLAUDE.md) *neden*i, TASKS.md *ne*yi ve *sırayı* anlatır.

**Repo:** https://github.com/aygunbayirdev/ecommerce-platform (public)

## Şu An Neredeyiz

**Faz 2 tamamlandı** — Identity adres defteri, Catalog (referans veriler + Product/Variant/Image), Inventory (+ projenin ilk modüller arası domain event akışı: Catalog→Inventory), Cart (+ projenin ilk cross-module READ composition'ı: Cart→Catalog senkron `ISender` query çağrısı), Order (+ projenin ilk çok-modüllü checkout orkestrasyonu: Order→Cart/Identity/Inventory, dağıtık transaction yerine sıralamayla çözülen atomiklik) ve **Payment modülü** (mock/test ödeme akışı — bkz. madde 9 aşağıda, gerçek gateway Stripe denendi ama Türkiye desteklenmediği için vazgeçildi, iyzico'ya Faz 7'de geçilecek) tamamlandı. Bir müşteri artık register olup, ürünlere bakıp, sepete ekleyip, adres seçip sipariş verip, (mock) ödeme yapabiliyor — Faz 2 bitiş kriteri karşılandı.

**Faz 3 tamamlandı** — **Promotion modülü** (kupon/indirim): `Coupon`/`CouponRedemption`, checkout orkestrasyonuna (`CreateOrderCommandHandler`) stok rezervasyonundan sonraki üçüncü adım olarak eklendi (Inventory'nin Reserve/Release deseninin kupon kullanımına uygulanışı). **Review modülü** (ürün yorumu/puanlama): satın alma doğrulaması Order+Catalog'u aynı anda okuyan ilk çok-modüllü okuma zinciri, moderasyon basit bir `IsApproved` boolean gate. **Shipping modülü** (kargo takibi): Order'daki `mark-shipped`/`mark-delivered` admin-only stand-in uçları gerçek çağrıcılarına kavuştu (`CreateShipmentCommandHandler`/`MarkShipmentDeliveredCommandHandler` mevcut `MarkOrderAsShippedCommand`/`MarkOrderAsDeliveredCommand`'ı çağırıyor — Payment'ın `MarkOrderAsPaidCommand`'ı çağırma deseninin dördüncü tekrarı, Order'da hiç yeni kod yazılmadı).

**Faz 4 tamamlandı** — projenin asıl öğrenme amacı: **Payment modülü gerçek, bağımsız bir mikroservise çıkarıldı** (Strangler Fig, bkz. madde 10 aşağıda). Yeni `payment-service/` kendi solution'ı, kendi Postgres'i (port 5434), kendi Docker deploy birimi olarak monolithten koptu; Order↔Payment arasındaki tüm iletişim artık RabbitMQ üzerinden asenkron entegrasyon eventleriyle (`OrderReadyForPaymentIntegrationEvent`/`PaymentSucceededIntegrationEvent`) yürüyor, hiçbir senkron çağrı kalmadı. Uçtan uca doğrulandı (dayanıklılık senaryosu dahil — payment-service kapalıyken oluşan sipariş, servis geri geldiğinde otomatik işlendi). Detaylı, öğretici bir anlatım için masaüstündeki `Faz4-Payment-Mikroservis-Gecisi.md` dosyasına bakılabilir. Bkz. TASKS.md Faz 4 detayları.

**Faz 5 sürüyor** — Frontend. Mimari kararlar netleşti (bkz. madde 11 aşağıda): tek Next.js projesi (App Router), `/admin` route bazlı ayrım, shadcn/ui (bu sürümde Radix değil Base UI tabanlı), WMS'ten taşınan zustand+axios auth deseni, herkese açık sayfalarda SSR (SEO için) + kişiselleştirilmiş sayfalarda Client Component. Proje iskeleti, **auth sayfaları** (login/register), **public katalog** (ana sayfa/kategori/ürün detay, üçü de SSR), **sepet** (guest+authenticated akış, header badge'i, ürün detayına "Sepete Ekle"), **adres defteri** (`account/addresses` CRUD), **checkout + ödeme** (adres seçimi, kupon kodu, sipariş oluşturma, payment-service'e doğrudan ödeme, onay ekranı) ve **siparişlerim + takip** (liste, detay/durum geçmişi, kargo takibi, iptal) tamamlandı, tarayıcıda uçtan uca doğrulandı. Son dört checkpoint (sepet, adres defteri, checkout, siparişlerim) backend'de hiç değişiklik gerektirmedi — Cart, Identity/Address, Order/Payment ve Shipping modülleri Faz 2-4'ten beri ihtiyaç duyulan her şeyi zaten sunuyordu. Siparişlerim checkpoint'i sırasında bulunan bir bug: `useAddresses`/`useCart`'ın zaten uyguladığı "önce `isAuthenticated` kontrol et, sonra sorguyu `enabled` ile kilitle" deseni `useMyOrders`/`useOrder`/`useShipment`'ta eksikti — sayfa kendi "giriş yapmalısınız" mesajını göstermeden önce hook zaten 401 alıp axios interceptor'ının sert `/login` yönlendirmesini tetikliyordu, üçüne de `enabled` parametresi eklenerek düzeltildi. Ondan önceki checkpoint'te (checkout) de iki gerçek bulgu çıkmıştı: seed edilen katalog ürünlerinde hiç stok yoktu (düzeltildi, test fixture) ve `usePayment`'ın polling'i tarayıcı sekmesi arka plandayken react-query'nin varsayılanı yüzünden duruyordu (`refetchIntervalInBackground: true` ile düzeltildi — gerçek bir UX açığıydı, sadece test ortamı garipliği değil). Ondan önceki iki checkpoint sırasında eksik çıkan iki parça tamamlanmıştı: CORS middleware (monolith + payment-service) ve "tüm ürünler" listesini dönen bir sorgu (Catalog'un `GetProductsQuery`'si artık `categoryId`'yi opsiyonel alıyor). Kalan checkpoint'ler (ürün yorumu, admin panel) sırayla ilerliyor — detay ve güncel durum için [TASKS.md](./TASKS.md) Faz 5.

## Geliştirme Döngüsü

Her görev için: **Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → `TASKS.md`'de işaretle.**

Kurallar:
- **Commit mesajları her zaman İngilizce yazılır** (proje dili Türkçe olsa da).
- Commit mesajlarına `Co-Authored-By` gibi Claude/Anthropic referansı eklenmez.
- Büyük/çok dosyalı işler (yeni modül gibi) önce plan olarak sunulur, onay alındıktan sonra koda geçilir.
- **Planlama mutlaka Plan Mode'da yapılır** — sohbet içine düz metin olarak plan yazıp onay istemek yerine, plan moduna geçilip plan orada sunulur, onay orada alınır. Sonrasında implementasyona geçilir.

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
2. Sonra **Payment modülü** (bkz. madde 3), gerçek, ayrı bir mikroservise çıkarıldı: ayrı deploy birimi, kendi veritabanı, monolith'le mesaj kuyruğu üzerinden asenkron haberleşen bağımsız bir servis (Faz 4, tamamlandı — bkz. madde 10).

Bu yaklaşım **Martin Fowler'ın Strangler Fig pattern'i** ve Sam Newman'ın "Monolith to Microservices" kitabının temel yaklaşımıdır — gerçek şirketlerin mikroservise geçiş şekli budur, "trend diye mikroservis yapmak" değildir. Bu, mülakatta "neden mikroservis" sorusuna savunulabilir bir cevap verir.

**Neden bu, hazmedilebilir?** Sıfırdan 4-5 servis + API gateway + service discovery + distributed tracing + saga orchestration hepsi birden öğrenilseydi bu seviye için çok fazla olurdu. Tek servis çıkarımıyla öğrenilecek yeni kavram seti küçük ve net: mesaj kuyruğu temelleri, servisler arası asenkron iletişim, eventual consistency, servis-özel veritabanı, bağımsız deploy.

### 3. Ayrılacak Servis: Ödeme (Payment) — kesinleşti, çıkarıldı

**Ödeme (Payment)** modülü ayrılacak servis olarak kesinleşti ve Faz 4'te fiilen çıkarıldı (bkz. madde 10). Gerekçe: gerçek sistemlerde ödeme genelde PCI-DSS izolasyonu ve farklı deploy/ölçeklenme ihtiyaçları yüzünden ayrı tutulur — bu, "neden bu servisi ayırdın" sorusuna gerçek bir cevap verir. Trivial bir servis (ör. "email/bildirim gönder") ayırmak yapay/zorlama dururdu, bilinçli olarak kaçınıldı.

Payment, Faz 2'de **önce monolith'in içinde** normal bir modül olarak inşa edildi (diğer modüllerden farksız), Faz 4'te strangler fig ile koparıldı.

### 4. Domain Event vs. Entegrasyon Eventi — net ayrım

- **Domain event** (monolith'in kendi modülleri arası, aynı process içinde — Catalog/Order/Inventory vb.): **MediatR** ile in-process dispatch edilir, outbox pattern ile güvenceye alınır (aynı process/transaction sınırı içinde kaldığı için). Identity modülünde bu akış zaten çalışıyor (`UserRegisteredDomainEvent` örneği).
- **Entegrasyon eventi** (monolith ↔ Payment.Service, process/deployment sınırını geçen): **RabbitMQ** üzerinden — Kafka değil, daha basit ve öğrenme eğrisi düşük. MediatR in-process bir mekanizma olduğu için servis sınırını aşamaz, bu yüzden mesaj kuyruğu gerekiyor. RabbitMQ, Payment ayrılana kadar (Faz 1-3) projeye hiç eklenmemişti; Faz 4'te docker-compose'a girdi (bkz. madde 10).

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
| **Review** | Ürün yorumu/puanlama (satın alma doğrulamalı) | Faz 3 tamamlandı |
| **Shipping** | Kargo takibi | Faz 3 tamamlandı |

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

### 10. Faz 4 — Payment'ın Mikroservise Çıkarılması: Uygulama Detayları

**Teknoloji seçimi (madde altındaki açık soru kapandı):** Payment.Service **aynı .NET stack**'i kullanıyor (mikroservisin "farklı teknoloji" avantajını göstermek için bilinçli olarak polyglot bir seçim yapılmadı). Gerekçe: bu projenin öğrenme hedefi mikroservis *mimarisi* (bağımsız deploy, kendi veritabanı, asenkron iletişim, eventual consistency) — dil/framework çeşitliliği ayrı bir öğrenme eğrisi ekler ve asıl hedefin üzerini örter. Gerçek şirketlerde de bir servisi ayırmanın ilk motivasyonu genelde organizasyonel/deploy sınırıdır, "farklı dil kullanabilelim" nadiren ilk sebeptir — bu seçim de savunulabilir.

**Event-carried state transfer:** Payment.Service, Order'a **hiçbir zaman** senkron sormuyor. `OrderReadyForPaymentIntegrationEvent` (OrderId, UserId, Amount taşır) geldiğinde `CreatePendingPaymentCommandHandler` bu veriyi kendi `payment.payments` tablosuna kopyalıyor; `ProcessPaymentCommandHandler` ödeme anında ownership/tutar kontrolünü tamamen bu lokal kopyadan yapıyor. Bu, CLAUDE.md madde 8'deki JWT kararının (stateless, çoklu servis için) somutlaştığı nokta — Payment.Service, Identity'ye hiç ağ çağrısı yapmadan aynı `Jwt:SigningKey` ile token'ı kendi doğruluyor.

**Outbox'ın ikinci kez, farklı bir taşıyıcıyla kullanılması:** Mevcut `OutboxMessage`/`OutboxWritingInterceptor` (Identity/Catalog'un MediatR'a dispatch eden `OutboxProcessor<TDbContext>`'i kullandığı altyapı) hiç değişmedi. Yeni `RabbitMqOutboxPublisher<TDbContext>` aynı tabloyu okuyup RabbitMQ'ya publish eden **paralel, ayrı** bir `BackgroundService` — mevcut, kanıtlanmış akışı riske atmadan ikinci bir taşıyıcı eklemenin yolu.

**Monorepo'da bağımsız solution — CPM keşif sınırı:** `Directory.Packages.props`, MSBuild tarafından sadece bir projenin **üst dizin zincirinde** aranıyor, kardeş dizinlere (`payment-service/` ↔ `backend/`) sızmıyor. Bu yüzden `payment-service/` kendi `Directory.Packages.props`/`Directory.Build.props`'una sahip — teknik bir zorunluluk ama aynı zamanda servisin gerçekten bağımsız derlendiğinin kanıtı olarak da okunabilir (paket sürümlerini istese bağımsız yükseltebilir).

**Docker build context — repo köküne genişleme:** `payment-service/PaymentService.slnx` (ve Payment'ın çıkarılmasıyla artık `backend/ECommercePlatform.slnx` da) kardeş dizinlerdeki `contracts/ECommercePlatform.IntegrationEvents`'a referans veriyor. Docker build context bir projenin kendi dizinine (`./backend`) daraltılamıyor — hem `backend/Dockerfile` hem `payment-service/Dockerfile` artık **repo kökünü** context alıyor (`docker-compose.yml`'de `context: .`, `dockerfile: backend/Dockerfile` / `payment-service/Dockerfile`), Dockerfile içindeki `dotnet restore`/`publish` yolları buna göre `backend/...`/`payment-service/...` öneki taşıyor.

**RabbitMQ topolojisi — bilinçli en basit seçim:** Varsayılan (isimsiz) exchange + routing key = kuyruk adı kullanılıyor, özel exchange/binding kurulmadı — ilk RabbitMQ entegrasyonu için kavram setini küçük tutma kararının (madde 5) doğrudan uygulanışı. `RabbitMqConsumerBackgroundService<TMessage>` başarısız mesajı nack+no-requeue ile düşürüyor; dead-letter queue bilinçli olarak bu fazın kapsamı dışında (Faz 6'da Testcontainers ile entegrasyon testleri gelince yeniden değerlendirilebilir).

**Uçtan uca doğrulanan dayanıklılık senaryosu:** `payment-service` container'ı durdurulmuş haldeyken oluşturulan bir siparişin `OrderReadyForPaymentIntegrationEvent`'i `order-ready-for-payment` kuyruğunda bekledi (`rabbitmqctl list_queues` ile 1 mesaj/0 tüketici gözlemlendi); servis tekrar ayağa kalkınca kuyruk otomatik boşaldı ve `Payment` kaydı gecikmeli ama doğru veriyle oluştu. Bu, senkron bir mimarinin (eski hâl: `ProcessPaymentCommandHandler` → `ISender.Send(GetOrderByIdQuery)`) hiçbir zaman veremeyeceği bir garanti — mesaj tabanlı entegrasyonun somut faydası, sadece teoride değil, canlı ortamda gösterildi.

**Kapsam dışı bırakılanlar (bilinçli sınırlar):** Gerçek veri migrasyonu (eski `payment` şeması boştu, taşınacak prod verisi yoktu — sadece `DROP SCHEMA` ile temizlendi), dead-letter queue, distributed tracing (OpenTelemetry vb.), saga/compensating transaction orchestration (Order→Payment akışı tek yönlü ve event-carried state transfer ile zaten basit; gerçek bir saga ihtiyacı ancak Payment'tan geriye Order'a *iptal* gerektiren bir akış eklenirse doğar, bu MVP'de yok). Detaylı, öğretici bir anlatım (mimari diyagramlar, RabbitMQ temelleri, mülakat konuşma noktaları) için masaüstündeki `Faz4-Payment-Mikroservis-Gecisi.md` dosyasına bakılabilir.

### 11. Faz 5 — Frontend: Teknoloji ve Mimari Kararları

**Proje yapısı — tek Next.js projesi, route bazlı ayrım:** `frontend/` altında tek proje; admin panel `/admin` route grubunda, geri kalan her şey public site. WMS'in "tek proje" felsefesinin devamı — ayrı bir admin deploy biriminin getirisi bu ölçekte (tek geliştirici, orta büyüklükte panel) gerekli görülmedi. Backend'deki modül-başına-şema izolasyonunun frontend karşılığı değil; bu sadece bir route/dosya organizasyonu kararı.

**Framework — Next.js (App Router), WMS'in Vite+React'inden bilinçli bir sapma:** WMS internal bir admin aracı olduğu için CSR (client-side rendering) yeterliydi. Bu proje ise public bir e-ticaret sitesi — ürün sayfalarının arama motorlarında indekslenmesi (SEO) gerçek bir iş ihtiyacı, ve App Router'ın Server Component'leri bunu ekstra bir SSR altyapısı kurmadan native sağlıyor. Aynı zamanda öğrenme açısından yeni bir stack denenmiş oldu (React Query + Zustand + shadcn/ui kısmı WMS ile aynı kaldı, sadece framework/routing katmanı değişti).

**Next.js 16 önemli notlar (proje bu sürümle kuruldu):** `params`/`searchParams` her zaman `Promise` (senkron erişim tamamen kaldırıldı — `await params` şart). `fetch` varsayılan olarak **cache'siz** (Next 15'te başlayan davranışın devamı) — sayfa render'ını bloke eder, `<Suspense>` ile stream edilebilir. `next lint` kaldırıldı, ESLint CLI doğrudan kullanılıyor (`npm run lint` → `eslint`). `middleware.ts` → `proxy.ts`'e yeniden adlandırıldı (bu proje middleware/proxy kullanmıyor — bkz. aşağıdaki auth notu). Turbopack varsayılan derleyici. Kaynak: `frontend/node_modules/next/dist/docs/` (proje köküne özel, sürüme göre güncel dokümantasyon — yeni bir Next major sürümüne geçilirse önce buradan okunmalı, training-data bilgisi güncel olmayabilir).

**Kimlik doğrulama — WMS deseninin birebir taşınması, middleware/proxy YOK:** Backend zaten stateless Bearer JWT için tasarlandı (madde 8) — cookie/session değil. Bu tutarlılığı korumak için Next.js'in `proxy.ts` (sunucu tarafı route guard) kullanılmadı, çünkü token client-side `localStorage`'da (zustand `persist` middleware ile) tutuluyor ve sunucu tarafından hiç görülmüyor. Bunun yerine WMS'teki `features/auth/store.ts` + `lib/axios.ts` deseni aynen taşındı: access/refresh token zustand'da, `axios` request interceptor'ı her isteğe `Authorization: Bearer` ekliyor, response interceptor'ı 401'de tek seferlik refresh+retry yapıp başarısızsa `/login`'e yönlendiriyor. İki backend origin'i (`apiClient` monolith, `paymentApiClient` payment-service) aynı interceptor mantığını paylaşıyor ama `refreshAccessToken` her zaman monolithin `/auth/refresh`'ine gidiyor — sadece Identity token üretip yeniliyor, payment-service'in kendi refresh endpoint'i yok (madde 10'daki "Payment.Service token'ı kendi doğruluyor ama üretmiyor" ayrımının doğal sonucu).

**SSR + client-side auth'un birlikte çalışması — sayfa bazında net ayrım:** Kimlik gerektirmeyen, herkese açık sayfalar (ana sayfa ürün grid'i, ürün detay, kategori) Server Component olarak yazılıyor, backend'in anonim `GET` endpoint'lerine native `fetch` ile sunucu tarafında istek atıyor — SEO kazanımının kaynağı burası. Kişiselleştirilmiş/mutasyon gerektiren sayfalar (sepet, checkout, siparişlerim, adres defteri, admin panel) Client Component (`'use client'`) olarak kalıyor, yukarıdaki axios+zustand+react-query deseniyle çalışıyor. İki yaklaşım aynı sayfada iç içe geçebiliyor (örn. ürün detay sayfası SSR ürün bilgisini gösterirken "sepete ekle" butonu Client Component).

**shadcn/ui bu projede Radix değil Base UI (`@base-ui/react`) kullanıyor — `asChild` yerine `render` prop:** shadcn'in güncel `base-nova` stili artık Radix yerine Base UI'a dayanıyor. Polimorfik render (bir bileşeni başka bir elemente, örn. `Button`'ı `Link`'e, dönüştürme) Radix'teki `asChild` + wrapped child yerine `render={<Link href="..." />}` prop'uyla yapılıyor. **Önemli detay:** `Button`/`DropdownMenuTrigger` gibi varsayılan olarak native `<button>` render eden bileşenler `<Link>` (bir `<a>`) gibi native-olmayan bir elemente render edildiğinde `nativeButton={false}` **açıkça** verilmeli, yoksa Base UI konsola erişilebilirlik uyarısı basıyor (`DropdownMenuItem` gibi varsayılan olarak `<div>` render eden bileşenlerde bu sorun yok, varsayılanları zaten `false`). Bu proje boyunca yeni bir polimorfik bileşen eklenirken bu noktaya dikkat edilmeli.

**Test/lint — WMS ile aynı test yığını, farklı lint aracı:** `vitest` + `@testing-library/react` (WMS ile birebir aynı, `vitest.config.mts` WMS'in `vite.config.ts`'sinin test bölümüyle aynı ayarlara sahip). Lint için WMS'in `oxlint`'i **değil**, Next.js'in kendi `eslint`+`eslint-config-next`'i kullanılıyor — `next lint` kaldırıldığı için (Next 16) zaten ayrı bir ESLint kurulumu şart, ve `eslint-config-next`'in App Router'a özel kuralları (Server/Client Component sınırları, `next/image` vb.) oxlint'te henüz yok.

**İki backend origin'i env değişkeniyle ayrı ayrı konfigüre ediliyor:** `NEXT_PUBLIC_API_URL` (monolith, çoğu şey) ve `NEXT_PUBLIC_PAYMENT_API_URL` (payment-service, sadece ödeme). Checkout akışında ödeme adımı **doğrudan** `paymentApiClient` ile payment-service'e gidiyor — Faz 4'ün "Payment.Service'e artık monolith üzerinden değil doğrudan gidilir" mimari kararının frontend'deki karşılığı, madde 10'daki backend tarafı kararla simetrik.

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

- **Deploy stratejisi** (TASKS.md Faz 6): **Hetzner'deki kendi sunucumuza deploy edilecek** — kesinleşti. Docker Compose ile mi, başka bir yöntemle mi olacağı henüz netleşmedi (Kubernetes şimdilik "gereksiz rabbit hole" olarak değerlendirildi). Faz 6 bitip MVP hazır olunca yayına alınacak.
- **CI/CD detayları** (TASKS.md Faz 6): hangi pipeline aracı, hangi aşamalar.
