# OpenCV ile Nesne Tanıma (.NET Framework 4.7.2)

Kısa: Windows Forms tabanlı, ONNX modelleri ile yerel (offline) çalışan görsel sınıflandırma uygulaması.

Gereksinimler
- Windows 10/11
- .NET Framework 4.7.2
- Visual Studio 2019/2022

Kurulum
1. Çözümü Visual Studio'da açın: `OpenCV.sln`
2. NuGet paketlerini ekleyin:
   - `OpenCvSharp4`
   - `OpenCvSharp4.runtime.win`
   (Package Manager Console):
   ```powershell
   Install-Package OpenCvSharp4
   Install-Package OpenCvSharp4.runtime.win
   ```
3. `Models` klasörüne ONNX modelini kopyalayın (ör. `mobilenetv2-7.onnx`) ve `imagenet_classes.txt` dosyasının bulunduğundan emin olun.
   - Proje içinde model dosyasını seçip **Build Action = Content** ve **Copy to Output Directory = Copy if newer** ayarlarını yapın.

Çalıştırma
1. Visual Studio'da F5 ile başlatın.
2. "Görsel Yükle" ile bir resim seçin.
3. "Analiz Et" ile tahmini çalıştırın; Top-5 sonuç MessageBox ile gösterilir.

Kısa notlar
- Model ve etiket dosyası yoksa uygulama uyarı verir.
- Büyük modeller CPU üzerinde yavaş çalışabilir; GPU desteği eklenmelidir.

Lisans
- Bu proje MIT lisansı altında dağıtılmaktadır. Detaylar `LICENSE` dosyasında bulunmaktadır.
