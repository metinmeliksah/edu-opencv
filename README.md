# OpenCV ile Görsel Tanýma (.NET Framework 4.7.2)

Kýsa: Windows Forms tabanlý, ONNX modelleri ile yerel (offline) çalýþan görsel sýnýflandýrma uygulamasý.

Gereksinimler
- Windows 10/11
- .NET Framework 4.7.2
- Visual Studio 2019/2022

Kurulum
1. Çözümü Visual Studio'da açýn: `OpenCV.sln`
2. NuGet paketlerini ekleyin:
   - `OpenCvSharp4`
   - `OpenCvSharp4.runtime.win`
   (Package Manager Console):
   ```powershell
   Install-Package OpenCvSharp4
   Install-Package OpenCvSharp4.runtime.win
   ```
3. `Models` klasörüne ONNX modelini kopyalayýn (ör. `mobilenetv2-7.onnx`) ve `imagenet_classes.txt` dosyasýnýn bulunduðundan emin olun.
   - Proje içinde model dosyasýný seçip **Build Action = Content** ve **Copy to Output Directory = Copy if newer** ayarlarýný yapýn.

Çalýþtýrma
1. Visual Studio'da F5 ile baþlatýn.
2. "Görsel Yükle" ile bir resim seçin.
3. "Analiz Et" ile tahmini çalýþtýrýn; Top-5 sonuç MessageBox ile gösterilir.

Kýsa notlar
- Model ve etiket dosyasý yoksa uygulama uyarý verir.
- Büyük modeller CPU üzerinde yavaþ çalýþabilir; GPU desteði eklenmelidir.

Lisans
Eðitim amaçlý. Model lisanslarýný kontrol edin.

Kýsa GitHub açýklamasý: "OpenCV ile Görsel Tanýma — Offline ONNX image classifier (Windows Forms, .NET 4.7.2)"
