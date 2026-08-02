# E-Ticaret Platformu — Proje Başlangıç Notları

Bu dosya henüz kod içermeyen bir projenin başlangıç kararlarını içerir. Bu kararlar başka bir konuşmada (WMS/Depo Yönetim Sistemi projesiyle ilgili konuşma çok uzadığı için) alındı, buradan devam edilecek.

## Geliştirme Döngüsü

Her görev için: **Planla → Implement et → Testleri güncelle/yaz → Dökümanları güncelle/yaz → Commit mesajı yaz ve review için sun → Onay al → Commit et → `TASKS.md`'de işaretle.**

Kurallar:
- **Commit mesajları her zaman İngilizce yazılır** (proje dili Türkçe olsa da).
- Commit mesajlarına `Co-Authored-By` gibi Claude/Anthropic referansı eklenmez.

**Not:** `TASKS.md` henüz yok — bu proje üzerinde çalışılacak ilk konuşmada birlikte planlanıp oluşturulacak.

## Amaç ve Bağlam

- Modüler monolith / Clean Architecture gibi kavramlar WMS projesinde uygulandı (bkz. `WarehouseManagementSystem/CLAUDE.md`) — bu proje kapsamında mikroservis mimarisi yeni öğreniliyor.
- Portföy projelerinin "gösteriş için yapılmış" değil, **gerekçeli ve savunulabilir** mimari kararlar içermesi önemli görülüyor.
- Bu proje, WMS'in (modüler monolith) tamamlayıcısı olarak tasarlandı: WMS "iyi yapılmış bir monolith" hikâyesini anlatıyor, bu proje "mikroservise ne zaman/neden geçilir" hikâyesini anlatacak. İkisi birlikte "hangi mimariyi ne zaman seçeceğini biliyorum" anlatısını kuruyor.
- Kullanıcı mikroservis mimarisini **hiç bilmiyor**, öğrenmek istiyor. İki kriteri var:
  1. **Hazmedilebilirlik**: Seviyesine göre, kafası karışmadan öğrenmek istiyor.
  2. **Savunulabilirlik**: Bir işveren projeye baktığında "bu adam saçmalamış" dememeli — mimari kararların gerçek bir gerekçesi olmalı.

## Alınan Mimari Kararlar

### 1. Domain: E-Ticaret

Bankacılık değil e-ticaret seçildi çünkü bankacılık domaini gereksiz regülasyon/compliance karmaşıklığı ekliyor ve asıl öğrenilmek istenen şeyin (mikroservis mimarisi) üzerini örtüyor. E-ticaret hem daha standart/iyi belgelenmiş bir domain hem de mikroservis için doğal servis sınırları sunuyor (Katalog, Sepet, Sipariş, Ödeme, Kargo, Stok gibi).

### 2. Yaklaşım: Modüler Monolith + Tek Servis Çıkarma (Strangler Fig)

**Sıfırdan çoklu mikroservis mimarisi KURULMAYACAK.** Bunun yerine:

1. Proje önce WMS'teki gibi bir **modüler monolith** olarak kurulacak (Clean Architecture, modül başına şema, CQRS — WMS'teki kanıtlanmış desen tekrar kullanılacak, kullanıcı bu deseni zaten biliyor, yeni öğrenilecek şey burada değil).
2. Sonra **tek bir modül** (bkz. aşağıda) gerçek, ayrı bir mikroservise çıkarılacak: kendi repo'su (ya da en azından kendi deploy birimi), kendi veritabanı, kendi container'ı, monolith'le mesaj kuyruğu üzerinden asenkron haberleşen bağımsız bir servis.

Bu yaklaşım **Martin Fowler'ın Strangler Fig pattern'i** ve Sam Newman'ın "Monolith to Microservices" kitabının temel yaklaşımıdır — gerçek şirketlerin mikroservise geçiş şekli budur, "trend diye mikroservis yapmak" değildir. Bu, mülakatta "neden mikroservis" sorusuna savunulabilir bir cevap verir.

**Neden bu, hazmedilebilir?** Sıfırdan 4-5 servis + API gateway + service discovery + distributed tracing + saga orchestration hepsi birden öğrenilseydi bu seviye için çok fazla olurdu. Tek servis çıkarımıyla öğrenilecek yeni kavram seti küçük ve net: mesaj kuyruğu temelleri, servisler arası asenkron iletişim, eventual consistency, servis-özel veritabanı, bağımsız deploy. WMS'ten bilinen her şeyi (CQRS, outbox, domain event, Clean Architecture) tekrar kullanmak, yeni öğrenilecek şeyleri sadece "mikroservise özgü" olanlarla sınırlıyor.

### 3. Ayrılacak Servis: Ödeme (Payment) — öneri, kesinleşmedi

Konuşmada örnek olarak **Ödeme (Payment)** modülü öne çıkarıldı, gerekçesi: gerçek sistemlerde ödeme genelde PCI-DSS izolasyonu ve farklı deploy/ölçeklenme ihtiyaçları yüzünden ayrı tutulur — bu, "neden bu servisi ayırdın" sorusuna gerçek bir cevap verir. Trivial bir servis (ör. "email/bildirim gönder") ayırmak yapay/zorlama dururdu, bilinçli olarak kaçınıldı.

**Bu henüz kesin karar değil** — sonraki konuşmada domain modelleme netleşince teyit edilmeli veya değiştirilmeli.

### 4. Domain Event vs. Entegrasyon Eventi — net ayrım (teyit edildi)

- **Domain event** (monolith'in kendi modülleri arası, aynı process içinde — WMS'teki gibi Catalog/Order/Inventory vb.): **MediatR** ile in-process dispatch edilir, WMS'teki gibi outbox pattern ile güvenceye alınabilir (aynı process/transaction sınırı içinde kaldığı için).
- **Entegrasyon eventi** (monolith → Payment mikroservisi, process/deployment sınırını geçen): **RabbitMQ** üzerinden — Kafka değil, daha basit ve öğrenme eğrisi düşük, bu proje kapsamı için yeterli. MediatR in-process bir mekanizma olduğu için servis sınırını aşamaz, bu yüzden mesaj kuyruğu gerekiyor.

Sipariş (Order) modülü Payment'a bir entegrasyon eventi yayınlayacak (RabbitMQ), Payment servisi bunu tüketip işleyecek — WMS'teki outbox+domain event akışına benzer bir mantık ama artık aynı process içinde değil, ayrı bir servise gidiyor.

### 5. Öğrenme Stratejisi (kurs vs. yaparak öğrenme)

Kullanıcı Fatih Çakıroğlu'nun ~40-50 saatlik Udemy mikroservis kursunu izleyip izlememe konusunda tereddütlü. Karar: **kursun tamamı baştan izlenmeyecek.**

- Sadece kursun giriş bölümü (mikroservis nedir, ne zaman kullanılır, monolith vs mikroservis tradeoff'ları) + RabbitMQ/mesaj kuyruğu bölümü izlenecek — bu kadarı kavram haritasını çizmeye yeter.
- Kursun geri kalanı (API gateway, service discovery, Kubernetes, çoklu servis orchestration vb.) bu proje kapsamında **hemen uygulanmayacağı için** şimdilik atlanacak — izlenip uygulanmayan bilgi kalıcı olmuyor.
- Sonra doğrudan e-ticaret monolith'ini kurmaya başlanacak; Payment'ı ayırma aşamasına gelindiğinde kursun ilgili bölümüne referans olarak dönülecek.
- Gerekçe: kullanıcı WMS'i (CQRS, outbox pattern, Clean Architecture) hiç kurs izlemeden, doğrudan inşa ederek + ihtiyaç oldukça araştırarak öğrendi. Bu, kendisi için kanıtlanmış bir öğrenme stili; mikroserviste de aynı stratejinin izlenmesi öneriliyor.

## Henüz Karar Verilmemiş — Sonraki Konuşmada Netleştirilecek

- Modüler monolith'in tam modül listesi (Katalog/Sepet/Sipariş/Stok/Ödeme örnek olarak geçti, kesinleşmedi)
- Backend teknoloji yığını (WMS .NET ile yapıldı, bu projede de aynısı mı kullanılacak yoksa farklı bir şey mi denenecek — konuşulmadı)
- **Frontend yapısı — iki taraf var, ilişkisi belirsiz:**
  - **Public taraf**: müşteri karşısı e-ticaret sitesi (Trendyol benzeri — ürün listeleme, sepet, sipariş verme).
  - **Admin panel**: WMS'teki gibi yönetim arayüzü.
  - Bu ikisi **tek bir frontend projesi** içinde mi olacak yoksa **ayrı iki proje** mi olacak — kesinleşmedi, gerekirse ayrılabilir.
  - UI kütüphanesi: WMS'te shadcn/ui kullanıldı, bu projede **birebir aynısı zorunlu değil**. Olası kombinasyonlar hepsi açık: (a) public ve admin'de aynı kütüphane (ör. ikisi de shadcn), (b) admin'de shadcn, public'te farklı bir kütüphane (public taraf genelde daha "marka kimliği" ağırlıklı tasarım istediği için farklı bir seçim mantıklı olabilir), (c) ikisinde de shadcn'den farklı bir şey. Hiçbiri seçilmedi, sonraki konuşmada karar verilecek.
- Ödeme servisinin teknoloji seçimi (aynı dil/framework mi, yoksa mikroservisin "farklı teknoloji kullanabilme" avantajını göstermek için bilinçli olarak farklı bir stack mi — polyglot persistence/polyglot programming örneği olabilir, konuşulmadı)
- Deploy stratejisi (Docker Compose yeterli mi, yoksa Kubernetes'e mi geçilecek — önceki konuşmada Kubernetes'in bu aşamada "gereksiz rabbit hole" olduğu belirtildi, ama kesin karar yok)
- Veri modeli / domain detayları (ürün kataloğu, sepet, sipariş akışı, ödeme akışı — hiçbiri henüz tasarlanmadı)
- Test stratejisi, CI/CD (WMS'te CI/CD önerildi ama başlanmadı — bu projede baştan mı kurulacak?)

## Referans

Aynı mimari felsefeyi paylaşan başka bir modüler monolith örneği (WMS/Depo Yönetim Sistemi) mevcut — Clean Architecture, CQRS, outbox pattern, naming standardı gibi konularda oradaki kararlara referans olarak bakılabilir, ama bu yeni proje için otomatik olarak kopyalanmamalı; her karar bu projenin kendi ihtiyaçlarına göre yeniden değerlendirilmeli.
