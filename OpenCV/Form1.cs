using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace OpenCV
{
    public partial class Form1 : Form
    {
        private string imagePath = "";
        private Net neuralNet;
        private string[] classLabels;
        private string[] classLabelsTr; // optional Turkish translations
        private const string MODEL_PATH = "Models\\mobilenetv2-7.onnx";
        private const string LABELS_PATH = "Models\\imagenet_classes.txt";
        private const string LABELS_TR_PATH = "Models\\imagenet_classes_tr.txt";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadModel();
        }

        /// <summary>
        /// ONNX modelini ve etiketleri yükler
        /// </summary>
        private void LoadModel()
        {
            try
            {
                lblStatus.Text = "Model yükleniyor...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                // Candidate paths to look for the model (exe output folder, project folder, repository root, explicit developer path)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string modelFullPath = Path.Combine(baseDir, MODEL_PATH);
                string labelsFullPath = Path.Combine(baseDir, LABELS_PATH);
                string labelsTrFullPath = Path.Combine(baseDir, LABELS_TR_PATH);

                // also check the project source path you mentioned
                string devModelPath = @"D:\Projeler\OpenCV\OpenCV\Models\mobilenetv2-7.onnx";
                string devLabelsPath = @"D:\Projeler\OpenCV\OpenCV\Models\imagenet_classes.txt";
                string devLabelsTrPath = @"D:\Projeler\OpenCV\OpenCV\Models\imagenet_classes_tr.txt";

                // Try candidates until we find an existing file. Improved resolver searches parents and repo root (.git)
                string chosenModelPath = null;
                string chosenLabelsPath = null;
                string chosenLabelsTrPath = null;

                // Helper: resolve relative path by checking baseDir, parent directories, and repository root if present
                string Resolve(string relativePath)
                {
                    // 1) check exact relative to baseDir
                    string attempt = Path.Combine(baseDir, relativePath);
                    if (File.Exists(attempt)) return attempt;

                    // 2) walk up parents from baseDir
                    DirectoryInfo di = new DirectoryInfo(baseDir);
                    while (di != null)
                    {
                        attempt = Path.Combine(di.FullName, relativePath);
                        if (File.Exists(attempt)) return attempt;
                        di = di.Parent;
                    }

                    // 3) try to find git repository root upwards from baseDir and check there
                    di = new DirectoryInfo(baseDir);
                    while (di != null)
                    {
                        var gitDir = Path.Combine(di.FullName, ".git");
                        if (Directory.Exists(gitDir))
                        {
                            attempt = Path.Combine(di.FullName, relativePath);
                            if (File.Exists(attempt)) return attempt;
                            break;
                        }
                        di = di.Parent;
                    }

                    return null;
                }

                chosenModelPath = Resolve(MODEL_PATH) ?? (File.Exists(devModelPath) ? devModelPath : null);
                chosenLabelsPath = Resolve(LABELS_PATH) ?? (File.Exists(devLabelsPath) ? devLabelsPath : null);
                chosenLabelsTrPath = Resolve(LABELS_TR_PATH) ?? (File.Exists(devLabelsTrPath) ? devLabelsTrPath : null);

                if (chosenModelPath == null)
                {
                    // Build message using string.Format to avoid nested-quote parsing issues
                    string msg = string.Format(
                        "Model dosyasý bulunamadý. Aranan yollar:\r\n\r\n1) {0}\r\n2) {1}\r\n3) {2}\r\n\r\n{3}",
                        modelFullPath,
                        Path.Combine(baseDir, "Models", Path.GetFileName(MODEL_PATH)),
                        devModelPath,
                        "Projeye model dosyasýný ekleyip Build Action = Content ve Copy to Output Directory = Copy if newer ayarlarýný yapýn, veya tam dosya yolunu belirtin."
                    );

                    MessageBox.Show(
                        msg,
                        "Model Bulunamadý",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    lblStatus.Text = "Model bulunamadý - Lütfen model dosyasýný ekleyin";
                    lblStatus.ForeColor = Color.Red;
                    return;
                }

                // load labels if available, otherwise use defaults
                if (chosenLabelsPath != null)
                    classLabels = File.ReadAllLines(chosenLabelsPath);
                else
                    classLabels = GenerateDefaultLabels();

                // try load Turkish translations (optional)
                if (chosenLabelsTrPath != null)
                {
                    try
                    {
                        var lines = File.ReadAllLines(chosenLabelsTrPath);
                        // accept if count matches or has at least as many lines as classLabels
                        if (lines.Length >= classLabels.Length)
                        {
                            // take only as many as needed
                            classLabelsTr = lines.Take(classLabels.Length).ToArray();
                        }
                        else
                        {
                            // insufficient translations: ignore but keep original labels
                            classLabelsTr = null;
                        }
                    }
                    catch
                    {
                        classLabelsTr = null;
                    }
                }

                // Load ONNX with the resolved absolute path
                neuralNet = CvDnn.ReadNetFromOnnx(chosenModelPath);

                lblStatus.ForeColor = Color.Green;
                // indicate whether Turkish labels were loaded
                string trInfo = (classLabelsTr != null) ? ", Türkçe etiketler yüklendi" : "";
                lblStatus.Text = $"Model baþarýyla yüklendi - {classLabels.Length} sýnýf yüklendi{trInfo}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Model yüklenirken hata oluþtu:\r\n\r\n{ex.Message}\r\n\r\nLütfen OpenCvSharp4 ve OpenCvSharp4.runtime.win paketlerinin yüklü olduðundan emin olun.",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text = "Model yüklenemedi!";
                lblStatus.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Görsel yükleme butonu
        /// </summary>
        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Görsel Dosyalarý|*.jpg;*.jpeg;*.png;*.bmp|Tüm Dosyalar|*.*";
                openFileDialog.Title = "Analiz edilecek görseli seçin";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    imagePath = openFileDialog.FileName;

                    try
                    {
                        // Görseli PictureBox'a yükle
                        Image img = Image.FromFile(imagePath);
                        pictureBoxImage.Image = img;

                        btnAnalyze.Enabled = true;
                        lblStatus.Text = "Görsel yüklendi - Analiz için hazýr";
                        lblStatus.ForeColor = Color.Green;
                        lblResult.Text = "Analiz Et butonuna\nbasýn";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Görsel yüklenirken hata oluþtu:\r\n\r\n{ex.Message}",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        lblStatus.Text = "Görsel yüklenemedi!";
                        lblStatus.ForeColor = Color.Red;
                    }
                }
            }
        }

        /// <summary>
        /// Görsel analizi yapan ana fonksiyon
        /// </summary>
        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(imagePath) || neuralNet == null)
            {
                MessageBox.Show("Lütfen önce bir görsel yükleyin ve modelin yüklendiðinden emin olun.", "Uyarý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                lblStatus.Text = "Analiz yapýlýyor...";
                lblStatus.ForeColor = Color.Orange;
                btnAnalyze.Enabled = false;
                Application.DoEvents();

                // Görseli OpenCV formatýnda yükle
                Mat image = Cv2.ImRead(imagePath);

                // Görsel ön iþleme - MobileNetV2 için: 224x224, RGB, normalize
                Mat blob = CvDnn.BlobFromImage(
                    image: image,
                    scaleFactor: 1.0 / 255.0,  // Normalizasyon [0-1]
                    size: new OpenCvSharp.Size(224, 224),
                    mean: new Scalar(0.485, 0.456, 0.406),  // ImageNet mean
                    swapRB: true,  // BGR -> RGB
                    crop: false
                );

                // Modele input ver
                neuralNet.SetInput(blob);

                // Tahmin yap (forward pass)
                Mat output = neuralNet.Forward();

                // Sonuçlarý iþle
                ProcessPredictions(output);

                // Bellek temizliði
                blob.Dispose();
                output.Dispose();
                image.Dispose();

                lblStatus.Text = "Analiz tamamlandý";
                lblStatus.ForeColor = Color.Green;
                btnAnalyze.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Analiz sýrasýnda hata oluþtu:\r\n\r\n{ex.Message}\r\n\r\nStack Trace:\r\n{ex.StackTrace}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text = "Analiz baþarýsýz!";
                lblStatus.ForeColor = Color.Red;
                btnAnalyze.Enabled = true;
            }
        }

        /// <summary>
        /// Model tahmin sonuçlarýný iþler ve ekranda gösterir
        /// </summary>
        private void ProcessPredictions(Mat predictions)
        {
            // Çýktýyý float dizisine çevir
            float[] probabilities = new float[predictions.Total()];
            predictions.GetArray(out probabilities);

            // Softmax uygula (olasýlýklarý normalize et)
            float[] softmaxProbs = Softmax(probabilities);

            // En yüksek olasýlýklý sýnýfý bul
            int maxIndex = 0;
            float maxProb = softmaxProbs[0];

            for (int i = 1; i < softmaxProbs.Length; i++)
            {
                if (softmaxProbs[i] > maxProb)
                {
                    maxProb = softmaxProbs[i];
                    maxIndex = i;
                }
            }

            // Top 5 sonuçlarý bul
            var topResults = softmaxProbs
                .Select((prob, index) => new { Probability = prob, Index = index })
                .OrderByDescending(x => x.Probability)
                .Take(5)
                .ToList();

            // Sonuçlarý göster - tercih sýrasýyla: Türkçe etiketler (varsa) -> orijinal etiketler -> sýnýf numarasý
            string className;
            if (classLabelsTr != null && maxIndex < classLabelsTr.Length)
                className = classLabelsTr[maxIndex];
            else if (maxIndex < classLabels.Length)
                className = classLabels[maxIndex];
            else
                className = $"Sýnýf #{maxIndex}";

            lblResult.Text = $"{className}\n\nGüven: {(maxProb * 100):F2}%";

            // Top 5'i message box ile göster
            StringBuilder top5Text = new StringBuilder();
            top5Text.AppendLine("En Olasý 5 Sonuç:");
            top5Text.AppendLine();

            for (int i = 0; i < topResults.Count; i++)
            {
                string label;
                int idx = topResults[i].Index;
                if (classLabelsTr != null && idx < classLabelsTr.Length)
                    label = classLabelsTr[idx];
                else if (idx < classLabels.Length)
                    label = classLabels[idx];
                else
                    label = $"Sýnýf #{idx}";

                top5Text.AppendLine($"{i + 1}. {label}");
                top5Text.AppendLine($"   -> {(topResults[i].Probability * 100):F2}%");
                top5Text.AppendLine();
            }

            MessageBox.Show(top5Text.ToString(), "Detaylý Sonuçlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Softmax fonksiyonu - çýktýlarý olasýlýk daðýlýmýna çevirir
        /// </summary>
        private float[] Softmax(float[] values)
        {
            float max = values.Max();
            float[] exp = values.Select(v => (float)Math.Exp(v - max)).ToArray();
            float sum = exp.Sum();
            return exp.Select(e => e / sum).ToArray();
        }

        /// <summary>
        /// Varsayýlan etiketler oluþturur
        /// </summary>
        private string[] GenerateDefaultLabels()
        {
            return Enumerable.Range(0, 1000).Select(i => $"Sýnýf {i}").ToArray();
        }

        /// <summary>
        /// Form kapanýrken kaynaklarý temizle
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            neuralNet?.Dispose();
            base.OnFormClosing(e);
        }
    }
}