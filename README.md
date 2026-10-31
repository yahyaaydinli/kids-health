# Psikolog Test App - Dikkat ve Odak Ölçüm Uygulaması 🎯

Çocukların dikkat, odaklanma yeteneklerini ve tepki sürelerini eğlenceli oyunlar aracılığıyla ölçen, değerlendiren ve analiz eden modern bir **WPF (.NET / C#)** masaüstü uygulamasıdır.

Bu uygulama; psikologlar, rehber öğretmenler ve ebeveynler için çocukların bilişsel performanslarını takip edebilecekleri test ve değerlendirme araçları sunar.

---

## 📌 Özellikler

* **🎮 İnteraktif Ölçüm Oyunları:**
  * **Hedef Avcısı:** Çocukların görsel dikkat, odaklanma süresi ve reaksiyon hızlarını ölçen özel modül.
* **📊 Detaylı Sonuç ve Analiz (Result Window):** Test sonrası reaksiyon süreleri, doğru/yanlış sayısı ve dikkat skorlarının raporlanması.
* **🏗️ Modüler ve Temiz Mimari:** MVVM (Model-View-ViewModel) deseni kullanılarak yazılmış, ölçeklenebilir ve bakımı kolay kod yapısı.
* **🧪 Test Edilebilirlik:** Birim testleri (Unit Tests) ile desteklenmiş güvenilir test mimarisi.

---

## 🛠️ Kullanılan Teknolojiler

* **Dil:** C#
* **Platform:** .NET / WPF (Windows Presentation Foundation)
* **Mimari:** MVVM (Model-View-ViewModel)
* **Arayüz (UI):** XAML

---

## 📁 Proje Klasör Yapısı

```text
PsikologTestApp/
│
├── Models/          # Veri modelleri ve iş mantığı yapıları
├── Views/           # XAML tabanlı kullanıcı arayüzü pencereleri (MainWindow, HedefAvcisiWindow, ResultWindow vb.)
├── ViewModels/      # Arayüz ile veri modelleri arasındaki bağlantıyı sağlayan ViewModel sınıfları
├── Services/        # Veri işleme, test ve hesaplama servisleri
└── Tests/           # Uygulama birim testleri (Unit Tests)
```
## Program Menüsü ve İçerik Görünümü
<img width="775" height="796" alt="image" src="https://github.com/user-attachments/assets/561afa70-e64e-4710-87ac-ac3b1c530964" />

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
* Windows 10 veya üzeri işletim sistemi
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (.NET Desktop Development iş yükü yüklü olmalıdır)
* .NET SDK (Projede kullanılan sürüme uygun)

### Adımlar

1. **Projeyi klonlayın:**
   ```bash
   git clone https://github.com/kullanici-adi/PsikologTestApp.git
   ```

2. **Projeyi Visual Studio ile açın:**
   `PsikologTestApp.csproj` veya `.sln` dosyasını Visual Studio ile açın.

3. **Bağımlılıkları yükleyin ve Derleyin:**
   * Visual Studio içerisinden `Build > Build Solution` (`Ctrl + Shift + B`) adımlarını takip edin.

4. **Uygulamayı Çalıştırın:**
   * `F5` tuşuna basarak uygulamayı başlatabilirsiniz.

---

## 🤝 Katkıda Bulunma

1. Bu depoyu çatallayın (Fork edin)
2. Yeni bir özellik dalı oluşturun (`git checkout -b feature/YeniOzellik`)
3. Değişikliklerinizi kaydedin (`git commit -m 'Yeni özellik eklendi'`)
4. Dalınıza itin (`git push origin feature/YeniOzellik`)
5. Bir Çekme İsteği (Pull Request) oluşturun

---

## 📄 Lisans

Bu proje MIT Lisansı ile lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakabilirsiniz.
