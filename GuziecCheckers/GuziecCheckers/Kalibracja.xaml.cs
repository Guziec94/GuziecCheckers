using Emgu.CV;
using Emgu.CV.Structure;
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