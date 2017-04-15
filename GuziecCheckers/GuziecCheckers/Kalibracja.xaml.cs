using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuziecCheckers
{
    /// <summary>
    /// Interaction logic for Kalibracja.xaml
    /// </summary>
    public partial class Kalibracja : Page
    {
        public static Thread t = null;

        #region Ciało wątku przetwarzającego obraz napływający z kamery
        private void w()
        {
            try
            {
                Capture kamera = new Capture(0);

                while (true)
                {
                    Mat matImage = kamera.QueryFrame();
                    Image<Bgr, byte> obraz = matImage.ToImage<Bgr, byte>();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        #endregion

        public Kalibracja()
        {
            InitializeComponent();

            #region Uruchamiamy wątek przetwarzający obraz z kamery
            t = new Thread(w);
            t.Start();
            #endregion
        }

        /// <summary>
        /// Funkcja wyszukująca we wskazanym obrazie okręgi znajdujące się w zdefiniowanym zakresie kolorystycznym, zdefeniowanej odległości pomiędzy pojedynczymi egzemplarzami oraz zdefiniowanym zakresie długości promienia
        /// </summary>
        /// <param name="img">Przeszukiwany obraz</param>
        /// <param name="min">Dolny zakres kolorystyki wyszukiwanych okręgów</param>
        /// <param name="max">Górny zakres kolorystyki wyszukiwanych okręgów</param>
        /// <param name="DistanceMin">Minimalna odległość pomiędzy środkami znajdywanych okręgów</param>
        /// <param name="radiusMin">Minimalna długość promienia wyszukiwanych okręgów</param>
        /// <param name="radiusMax">Maksymalna długość promienia wyszukiwanych okręgów</param>
        /// <returns></returns>
        private CircleF[] findCircles(Image<Bgr, byte> img, Bgr min, Bgr max, double DistanceMin = 20.0, int radiusMin = 20, int radiusMax = 30)
        {
            UMat uimage = new UMat();
            CvInvoke.CvtColor(img.InRange(min, max).Convert<Bgr, byte>(), uimage, ColorConversion.Bgr2Gray);

            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            double cannyThreshold = 100;
            double circleAccumulatorThreshold = 100;
            CircleF[] circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, DistanceMin, cannyThreshold, circleAccumulatorThreshold, radiusMin, radiusMax);

            return circles;
        }

        /// <summary>
        /// Funkcja kalibrująca zmiany położenia szachownicy względem kamery
        /// </summary>
        /// <param name="img">Przeszukiwany obraz</param>
        /// <param name="size">Rozmiar szachownicy (Liczność pól szerokości długości umniejszona o 1 w obu przypadkach)</param>
        /// <param name="draw">Rysuje punkty wskazujące położenie pól szachownicy</param>
        /// <returns></returns>
        private VectorOfPointF calibrationCamera(Image<Bgr, byte> img, Size? size = null, bool draw = false)
        {
            Size patternSize = (Size)(size.HasValue ? size : new Size(9, 9));

            VectorOfPointF corners = new VectorOfPointF();
            bool found = CvInvoke.FindChessboardCorners(img, patternSize, corners);

            if(draw) CvInvoke.DrawChessboardCorners(img, patternSize, corners, found);

            return corners;
        }
    }

    #region Klasa z metodami ułatwiającymi konwersję obrazu
    public static class BitmapSourceConvert
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ImageToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static Bitmap ImageControlToBitmap(System.Windows.Controls.Image image)
        {
            RenderTargetBitmap rtBmp = new RenderTargetBitmap((int)image.ActualWidth, (int)image.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            image.Measure(new System.Windows.Size((int)image.ActualWidth, (int)image.ActualHeight));
            image.Arrange(new System.Windows.Rect(new System.Windows.Size((int)image.ActualWidth, (int)image.ActualHeight)));

            rtBmp.Render(image);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream stream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(rtBmp));

            encoder.Save(stream);
            return new Bitmap(stream);
        }
    }
    #endregion

}