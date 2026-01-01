📅 Haftalık Akıllı İlaç Kutusu (Weekly Smart Medicine Box)

Bu proje, düzenli ilaç kullanan hastaların ilaç saatlerini takip etmelerini sağlayan, Arduino tabanlı bir donanım ve C# WPF ile geliştirilmiş bir masaüstü kontrol uygulamasından oluşmaktadır.

Sistem, haftalık planlama yapılmasına olanak tanır ve ilgili gün/saat geldiğinde otomatik olarak doğru ilaç kutusunu (Sabah veya Akşam) açar.

🌟 Özellikler
🖥️ Masaüstü Uygulaması (C# WPF)

Modern Arayüz: Kullanıcı dostu, kart yapılı şık tasarım.

Haftalık Planlama: Haftanın 7 günü için ayrı ayrı alarm kurabilme.

Kısıtlama Kontrolü: Her gün için en fazla 2 alarm (Sabah/Akşam) ekleme güvenliği.

Seri Haberleşme: Arduino ile USB (Serial Port) üzerinden otomatik bağlantı.

Canlı Takip: Anlık saat ve bağlantı durumu göstergesi.

Ayar Yönetimi: İlaç alınmazsa çalacak alarmın bekleme süresini arayüzden değiştirme.

🤖 Donanım (Arduino)

Çift Hazne Kontrolü: 2 adet Servo motor ile Sabah ve Akşam kutularını ayrı ayrı kontrol eder.

Yumuşak Hareket: Servo motorlar kapakları sertçe değil, yavaş ve güvenli bir hızda açar/kapatır.

Sesli ve Işıklı Uyarı:

🟢 Yeşil LED: İlaç saati geldi, kutu açık.

🔴 Kırmızı LED: İlaç alınmadı veya alarm modu.

🔊 Buzzer: İlaç saati uyarısı ve süre aşımı alarmı.

Güvenlik: Belirlenen sürede ilaç alınmazsa (kutu kapatılmazsa) yüksek sesli alarm moduna geçer.

Manuel Kontrol: Fiziksel buton ile kutuyu kapatma ve alarmı susturma.

🛠️ Donanım Gereksinimleri ve Bağlantı Şeması

Proje için aşağıdaki bileşenlere ihtiyacınız vardır:

Bileşen	Pin (Arduino)	Açıklama
Arduino Uno/Nano	USB	Ana kontrolcü
Servo Motor 1	D9	Sabah Kutusu
Servo Motor 2	D10	Akşam Kutusu
Buton	D2	Kutuyu kapatmak için (Pull-up)
Kırmızı LED	D12	Uyarı Işığı
Yeşil LED	D8	Bilgi Işığı
Buzzer	D7	Sesli Uyarı

Not: Servo motorlar harici bir güç kaynağı ile beslenmesi önerilir, ancak tekli kullanımlarda Arduino 5V çıkışı yeterli olabilir.

🚀 Kurulum
1. Arduino Kısmı

Arduino klasöründeki .ino uzantılı dosyayı açın.

Servo kütüphanesinin yüklü olduğundan emin olun.

Kodu Arduino kartınıza yükleyin.

Devreyi yukarıdaki pin şemasına göre kurun.

2. C# (Windows) Kısmı

Projeyi Visual Studio ile açın.

MainWindow.xaml.cs dosyasını açın.

Şu satırı bulun ve kendi Arduino'nuzun bağlı olduğu port ile değiştirin:

code
C#
download
content_copy
expand_less
string sabitPortAdi = "COM3"; // COM3, COM4 vb. olabilir.

(Bu adımı Aygıt Yöneticisi'nden kontrol edebilirsiniz).

Projeyi derleyin ve çalıştırın (F5).

📖 Kullanım

Bağlantı: Uygulama açıldığında otomatik bağlanmayı dener. Bağlanmazsa "Bağlan" butonuna basın.

Alarm Ekleme:

Günü seçin (Örn: Pazartesi).

Saati ve Dakikayı girin.

Kutu numarasını seçin (1: Sabah, 2: Akşam).

"Listeye Ekle" butonuna basın.

Alarm Anı:

Zaman geldiğinde PC, Arduino'ya sinyal gönderir.

İlgili kutu yavaşça açılır, Yeşil LED yanar ve Buzzer öter.

Kapatma:

İlacı aldıktan sonra kutu üzerindeki Butona basın.

Kutu yavaşça kapanır ve sistem bir sonraki alarmı bekler.

Unutulursa:

Eğer kutu açık kalırsa (Varsayılan 60 sn), sistem Alarm Moduna geçer (Kırmızı LED + Sürekli Ses).

📸 Ekran Görüntüleri

![IMG20260101175045](https://github.com/user-attachments/assets/a10e76e5-f465-4596-9f73-db64dd92b6e8)

<img width="467" height="744" alt="Screenshot 2026-01-01 181516" src="https://github.com/user-attachments/assets/8cb464df-d241-48f6-b2f0-6e0fd6430658" />


🤝 Katkıda Bulunma

Bu projeyi Fork'layın.

Yeni bir özellik dalı (feature branch) oluşturun (git checkout -b yeni-ozellik).

Değişikliklerinizi Commit'leyin (git commit -m 'Yeni özellik eklendi').

Dalı Push'layın (git push origin yeni-ozellik).

Bir Pull Request oluşturun.
