# OpenCV ile G�rsel Tan�ma (.NET Framework 4.7.2)

K�sa: Windows Forms tabanl�, ONNX modelleri ile yerel (offline) �al��an g�rsel s�n�fland�rma uygulamas�.

Gereksinimler
- Windows 10/11
- .NET Framework 4.7.2
- Visual Studio 2019/2022

Kurulum
1. ��z�m� Visual Studio'da a��n: `OpenCV.sln`
2. NuGet paketlerini ekleyin:
   - `OpenCvSharp4`
   - `OpenCvSharp4.runtime.win`
   (Package Manager Console):
   ```powershell
   Install-Package OpenCvSharp4
   Install-Package OpenCvSharp4.runtime.win
   ```
3. `Models` klas�r�ne ONNX modelini kopyalay�n (�r. `mobilenetv2-7.onnx`) ve `imagenet_classes.txt` dosyas�n�n bulundu�undan emin olun.
   - Proje i�inde model dosyas�n� se�ip **Build Action = Content** ve **Copy to Output Directory = Copy if newer** ayarlar�n� yap�n.

�al��t�rma
1. Visual Studio'da F5 ile ba�lat�n.
2. "G�rsel Y�kle" ile bir resim se�in.
3. "Analiz Et" ile tahmini �al��t�r�n; Top-5 sonu� MessageBox ile g�sterilir.

K�sa notlar
- Model ve etiket dosyas� yoksa uygulama uyar� verir.
- B�y�k modeller CPU �zerinde yava� �al��abilir; GPU deste�i eklenmelidir.

Lisans
E�itim ama�l�. Model lisanslar�n� kontrol edin.

K�sa GitHub a��klamas�: "OpenCV ile G�rsel Tan�ma � Offline ONNX image classifier (Windows Forms, .NET 4.7.2)"
