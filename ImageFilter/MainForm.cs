using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageFilter
{
    public partial class MainForm : Form
    {
        // оригинальное изображение
        private Image<Bgr, byte> originalImage;
        // обработанное изображение
        private Image<Bgr, byte> processedImage;
        // флаг, который показывает какой сейчас изображение на экране (обработанное или оригинальное)
        private bool isOriginal = true;

        // изображение которое выводится на экран типа Bitmap
        private Bitmap drawingImage;

        // поле для окна с настройками фильтров
        private Controls controlsForm;

        // фабрика NumericUpDown нужна для создание объектов NumericUpDown с нужными параметрами
        private static NumericUpDown BuildNumeric(decimal value, decimal min, decimal max, double step = 1)
        {
            // создаем новый объект NumericUpDown
            NumericUpDown numeric = new NumericUpDown();
            // присваиваем значения
            numeric.Value = value;
            numeric.Minimum = min;
            numeric.Maximum = max;
            numeric.Increment = (decimal)step;
            // возвращаем сгенерированный объект
            return numeric;
        }

        // словарь с объектами FilterControl
        // определяет для какого фильтра, какие нужны элементы управления
        private Dictionary<int, List<FilterControl>> filterControls = new Dictionary<int, List<FilterControl>>{
            { -1, null},
            { 0, new List<FilterControl> {
                new FilterControl("X order", BuildNumeric(1, 0, 2, 1)),
                new FilterControl("Y order", BuildNumeric(0, 0, 2, 1)),
                new FilterControl("Размер апертуры", BuildNumeric(3, 0, 31, 2))
            }},
            { 1, new List<FilterControl> {
                new FilterControl("Размер апертуры", BuildNumeric(1, 0, 31, 2))
            }},
            { 2, new List<FilterControl> {
                new FilterControl("Размер апертуры", BuildNumeric(3, 0, 31, 2))
            }},
            { 3, new List<FilterControl>{
                new FilterControl("Размер апертуры", BuildNumeric(5, 0, 31, 2))
            }},
            { 4, new List<FilterControl>{
                new FilterControl("Размер апертуры", BuildNumeric(5, 0, 31, 2))
            }},
        };

        // текст который будет отображаться на кнопке, которая меняет изображения
        public string OriginalButtonText
        {
            // если isOriginal = true, то текст Оригинал, иначе Обработанное
            get { return isOriginal ? "Оригинал" : "Обработанное"; }
        }


        // Конструктор класса главного Окна
        public MainForm()
        {
            InitializeComponent();

            // создаем новый оъект окна с настройками, передаем ему пустую конфигурацию элементов управления и класс главного окна
            controlsForm = new Controls(null, this);
        }

        // функция которая выводит изображение
        private bool ShowImage()
        {
            // обрабатываем событие, если картинка еще не загружена
            try
            {
                // если у нас сейчас флаг isOriginal = true
                if (isOriginal)
                    // отрисовываем оригинальное изображение
                    drawingImage = originalImage.ToBitmap();
                else
                    // иначе обработанное
                    drawingImage = processedImage.ToBitmap();

                // выводи на экран
                pictureBox.Image = drawingImage;
            }
            catch (NullReferenceException)
            {
                // если произошла ошибка выводим сообщение об этом        
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // инвертируем флаг isOriginal, так как не удалось отрисовать изображение
                return !isOriginal;
            }

            //возвращаем флаг
            return isOriginal;
        }

        // Функция, которая показывает оригинальное изображение
        private void ShowOriginal()
        {
            isOriginal = true;
            isOriginal = ShowImage();
            toggleOriginal.Text = OriginalButtonText;
        }

        // Функция, которая показывает обработанное изображение
        private void ShowProcessed()
        {
            isOriginal = false;
            isOriginal = ShowImage();
            toggleOriginal.Text = OriginalButtonText;
        }

        //обработка события нажатия на кнопку Открыть
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //вызвать окно открытия файла и записать результат в dialogResult
            var dialogResult = openFileDialog.ShowDialog();

            // если пользователь выбрал файл
            if(dialogResult == DialogResult.OK)
            {
                // загружаем из файла изображение в originalImage
                originalImage = new Image<Bgr, byte>(openFileDialog.FileName);
                // копируем изображение из originalImage в processedImage
                processedImage = originalImage.Copy();

                // показываем оригинал
                ShowOriginal();
                // изменяем текст кнопки
                toggleOriginal.Text = OriginalButtonText;
            }
        }

        // обработка нажатия на кнопку Оригинал
        private void оригиналToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowOriginal();
        }

        // обработка нажатия на кнопку Обработанное
        private void обработанноеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowProcessed();
        }

        // обработка нажатия на кнопку Оригинал/Обработанное
        private void toggleOriginal_Click(object sender, EventArgs e)
        {
            // инвертируем значениее флага
            isOriginal = !isOriginal;
            isOriginal = ShowImage();
            
            toggleOriginal.Text = OriginalButtonText;
        }

        // обновляем конфигурацию элементов управления в окне Настроек, в зависимости от выбранного фильтра
        // index - номер фильтра, совпадающий с номером конфигурации
        private void UpdateControls(int index)
        {
            controlsForm.Update(filterControls[index], index);
        }

        // Вызываем фильтры с параметрами
        // index - номер фильтра, совпадающий с номером конфигурации
        // List<double> args - список аргументов
        public void ReprocessWithParams(int index, List<double> args)
        {
            try
            {
                switch (index)
                {
                    // Собель
                    case 0:
                        processedImage = originalImage.Convert<Gray, float>()
                            .Sobel((int)args[0], (int)args[1], (int)args[2])
                            .AbsDiff(new Gray(0.0))
                            .Convert<Bgr, byte>();
                        break;
                    // Лаплас
                    case 1:
                        processedImage = originalImage.Laplace((int)args[0]).Convert<Bgr, byte>();
                        break;
                    // Прюитт
                    case 2:
                        processedImage = Prewitt(originalImage, (int)args[0]);
                        break;
                    // Гаусс
                    case 3:
                        processedImage = originalImage.SmoothGaussian((int)args[0]);
                        break;
                    // Median
                    case 4:
                        processedImage = originalImage.SmoothMedian((int)args[0]);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            // показываем обработанное изображение
            ShowProcessed();
        }

        // обработка нажатия на кнопку Собель
        private void собельToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = originalImage.Convert<Gray, float>()
                    .Sobel(1, 0, 3)
                    .AbsDiff(new Gray(0.0))
                    .Convert<Bgr, byte>();
                UpdateControls(0);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // обработка нажатия на кнопку Лаплас
        private void лапласToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = originalImage.Laplace(1).Convert<Bgr, byte>();
                UpdateControls(1);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // обработка нажатия на кнопку Прюитт
        private void преввитToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = Prewitt(originalImage, 3);
                UpdateControls(2);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // обработка нажатия на кнопку Размытие по Гауссу
        private void размытиеПоГауссуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = originalImage.SmoothGaussian(5);
                UpdateControls(3);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // обработка нажатия на кнопку Median Размытие
        private void medianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = originalImage.SmoothMedian(5);
                UpdateControls(4);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // обработка нажатия на кнопку Черно-белое
        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = originalImage.Convert<Gray, byte>().Convert<Bgr, byte>();
                UpdateControls(-1);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                processedImage = Sepia(originalImage);
                UpdateControls(-1);
                ShowProcessed();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Сначала откройте изображение!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        //фильтр Прюитта
        private Image<Bgr, byte> Prewitt(Image<Bgr, byte> img, int aperture)
        {
            // двумерные массивы для инициализации ядер оператора Прюитта
            float[,] arrayX = new float[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 }, };
            float[,] arrayY = new float[,] { { -1, -1, -1 }, { 0, 0, 0 }, { 1, 1, 1 } };

            // горизонтальное ядро
            ConvolutionKernelF kernelX = new ConvolutionKernelF(arrayX);
            // пертикальное ядро
            ConvolutionKernelF kernelY = new ConvolutionKernelF(arrayY);

            // Получаем ч/б изображение
            Image<Gray, byte> grayImage = new Image<Gray, byte>(img.Bitmap);
            // Изображения для размытия по гауссу
            Image<Gray, float> gauss = new Image<Gray, float>(img.Size);
            // результирующее изображение
            Image<Bgr, byte> finalImage = new Image<Bgr, byte>(img.Size);

            //применяем фильтр гаусса с заданной апертурой
            CvInvoke.GaussianBlur(grayImage.Mat, gauss.Mat, new Size(aperture, aperture), 0);
            // применяем ядра к полученному размытому изображению
            Image<Gray, float> imageX = gauss * kernelY;
            Image<Gray, float> imageY = gauss * kernelX;

            // складываем X и Y составляющую изображения
            finalImage = (imageX + imageY).AbsDiff(new Gray(0.0)).Convert<Bgr, byte>();

            // очищаем память
            imageX.Dispose();
            imageY.Dispose();
            gauss.Dispose();
            grayImage.Dispose();

            // возвращаем изображение
            return finalImage;
        }

        private Image<Bgr, byte> Sepia(Image<Bgr, byte> image)
        {
            float[,] array = { { 0.272f, 0.534f, 0.131f }, { 0.349f, 0.686f, 0.168f }, { 0.393f, 0.769f, 0.189f } };

            Image<Bgr, byte> img = image.Copy();

            ConvolutionKernelF kernel = new ConvolutionKernelF(array);
            
            CvInvoke.Transform(img.Mat, img.Mat, kernel);

            return img;
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controlsForm.Show();
        }

        //функция для получения кодека по названию
        private ImageCodecInfo getEncoderInfo(string mimeType)
        {
            //получить все кодеки
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            //найти корректный кодек
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            //если не найден вернуть null
            return null;
        }

        private void SaveProcessed(string path)
        {
            string extension = path.Split('.').Last();

            EncoderParameter encoder = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);
            ImageCodecInfo codec = null;

            switch (extension)
            {
                case "jpg":
                    codec = getEncoderInfo("image/jpeg");
                    break;
                case "jpeg":
                    codec = getEncoderInfo("image/jpeg");
                    break;
                case "png":
                    codec = getEncoderInfo("image/png");
                    break;
                case "bmp":
                    codec = getEncoderInfo("image/bitmap");
                    break;
                case null:
                    MessageBox.Show("Неизвестный формат файла.\nСохранение не удалось!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
            }

            if (codec == null)
            {
                MessageBox.Show("Неизвестный кодек.\nСохранение не удалось!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            EncoderParameters encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = encoder;

            processedImage.ToBitmap().Save(path, codec, encoderParameters);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Перезаписать исходное изображение?", "Сохранить", MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    SaveProcessed(openFileDialog.FileName);
                    break;
                case DialogResult.No:
                    var saveResult = saveFileDialog.ShowDialog();
                    if (saveResult == DialogResult.OK)
                        SaveProcessed(saveFileDialog.FileName);
                    break;
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveResult = saveFileDialog.ShowDialog();
            if (saveResult == DialogResult.OK)
                SaveProcessed(saveFileDialog.FileName);
        }

        // закрываем изображение
        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            originalImage = null;
            processedImage = null;
            drawingImage = null;
            pictureBox.Image = null;
            controlsForm.Hide();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (processedImage == null)
            {
                e.Cancel = false;
                return;
            } 

            var result = MessageBox.Show("Хотите сохранить изменения?", "Выход", MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    сохранитьКакToolStripMenuItem_Click(this, new EventArgs());
                    e.Cancel = false;
                    break;
                case DialogResult.No:
                    e.Cancel = false;
                    break;
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
