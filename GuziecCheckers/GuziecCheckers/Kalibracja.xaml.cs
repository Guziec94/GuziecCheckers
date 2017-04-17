using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
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
                Chessboard szachownica = new Chessboard(new Bgr(), new Bgr(), new Bgr(), new Bgr(), 20.0, 20.0, 20, 20, 30, 30);
                Capture kamera = new Capture(0);

                while (true)
                {
                    Mat matImage = kamera.QueryFrame();
                    Image<Bgr, byte> obraz = matImage.ToImage<Bgr, byte>();

                    szachownica.Calibration(obraz, true);
                   
                    imgPodgladSzachownicy.Dispatcher.Invoke(() => { imgPodgladSzachownicy.Source = Tools.ImageToBitmapSource(obraz); });
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
    public static class Tools
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

    public class Chessboard
    {
        #region Struktury itp
        private struct Field
        {
            public char column { get; set; }
            public int row { get; set; }

            public Point leftUp { get; set; }
            public Point leftDown { get; set; }
            public Point rightUp { get; set; }
            public Point rightDown { get; set; }

            public Field(char column, int row, Point leftUp, Point rightUp, Point leftDown, Point rightDown)
            {
                this.column = column;
                this.row = row;

                this.leftUp = leftUp;
                this.rightUp = rightUp;
                this.leftDown = leftDown;
                this.rightDown = rightDown;
            }
        }

        private struct PawnsInfo
        {
            public static Bgr minColorRange1 { get; set; }
            public static Bgr maxColorRange1 { get; set; }
            public static int minRadius1 { get; set; }
            public static int maxRadius1 { get; set; }
            public static double minDistance1 { get; set; }

            public static Bgr minColorRange2 { get; set; }
            public static Bgr maxColorRange2 { get; set; }
            public static int minRadius2 { get; set; }
            public static int maxRadius2 { get; set; }
            public static double minDistance2 { get; set; }
        }
        #endregion
        private int _size;
        private List<Field> _fields;

        private CircleF[] _pawnsPlayer1;
        private CircleF[] _pawnsPlayer2;

        /// <summary>
        /// Metoda kalibrująca zmiany położenia pól szachownicy względem kamery oraz zmiany ułożenia pionów na szachownicy
        /// </summary>
        /// <param name="img">Przeszukiwany obraz</param>
        /// <param name="draw">Rysuje punkty wskazujące położenie pól szachownicy</param>
        /// <returns></returns>
        public void Calibration(Image<Bgr, byte> img, bool drawFields = false, bool drawPawnsPlayer1 = false, bool drawPawnsPlayer2 = false)
        {
            #region Kalibracja kamery
            Size patternSize = new Size((_size - 1), (_size - 1));

            VectorOfPointF corners = new VectorOfPointF();
            bool found = CvInvoke.FindChessboardCorners(img, patternSize, corners);
            #endregion
            #region Aktualizacja położenia pól
            if (corners.Size == Convert.ToInt32(Math.Pow((_size - 1), 2)))
            {
                _fields.Clear();
          
                char column = 'A';
                int row = 1;

                for (int i = 0; i < corners.Size - (_size - 1); i++)
                {
                    if ((i + 1) % (_size - 1) == 0) { column = (char)(Convert.ToUInt16(column) + 1); continue; }
                    _fields.Add(new Field(column, row++, new Point((int)corners[i].X, (int)corners[i].Y), new Point((int)corners[i + 1].X, (int)corners[i + 1].Y), new Point((int)corners[i + (_size - 1)].X, (int)corners[i + (_size - 1)].Y), new Point((int)corners[i + _size].X, (int)corners[i + _size].Y)));

                    if (row == (_size - 1)) row = 1;
                }
            }
            #endregion
            #region Aktualizacja położenia pionów

            double cannyThreshold = 100;
            double circleAccumulatorThreshold = 100;

            UMat uimage = new UMat();
            CvInvoke.CvtColor(img.InRange(PawnsInfo.minColorRange1, PawnsInfo.maxColorRange1).Convert<Bgr, byte>(), uimage, ColorConversion.Bgr2Gray);

            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);
                 
            _pawnsPlayer1 = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, PawnsInfo.minDistance1, cannyThreshold, circleAccumulatorThreshold, PawnsInfo.minRadius1, PawnsInfo.maxRadius1);

            uimage = new UMat();
            CvInvoke.CvtColor(img.InRange(PawnsInfo.minColorRange2, PawnsInfo.maxColorRange2).Convert<Bgr, byte>(), uimage, ColorConversion.Bgr2Gray);

            pyrDown = new UMat();
            CvInvoke.PyrDown(uimage, pyrDown);
            CvInvoke.PyrUp(pyrDown, uimage);

            _pawnsPlayer2 = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, PawnsInfo.minDistance2, cannyThreshold, circleAccumulatorThreshold, PawnsInfo.minRadius2, PawnsInfo.maxRadius2);
            #endregion

            #region Wyświetlanie pól i pionów
            if (drawFields) CvInvoke.DrawChessboardCorners(img, patternSize, corners, found);

            if (drawPawnsPlayer1)
                foreach (CircleF pawn in _pawnsPlayer1) img.Draw(pawn, new Bgr(0, 255, 0), 1);
            if (drawPawnsPlayer2)
                foreach (CircleF pawn in _pawnsPlayer2) img.Draw(pawn, new Bgr(0, 255, 0), 1);
            #endregion
        }

        /// <summary>
        /// Konstruktor klasy reprezentującej obiekt szachownicy
        /// </summary>
        /// <param name="minColorRange1">Dolny zakres koloru pionów pierwszego gracza</param>
        /// <param name="minColorRange2">Dolny zakres koloru pionów drugiego gracza</param>
        /// <param name="maxColorRange1">Górny zakres koloru pionów pierwszego gracza</param>
        /// <param name="maxColorRange2">Górny zakres koloru pionów drugiego gracza</param>
        /// <param name="minDistance1">Minimalny dystans pomiędzy pionami pierwszego gracza</param>
        /// <param name="minDistance2">Minimalny dystans pomiędzy pionami drugiego gracza</param>
        /// <param name="minRadius1">Dolny zakres długości promienia pionów pierwszego gracza</param>
        /// <param name="minRadius2">Dolny zakres długości promienia pionów drugiego gracza</param>
        /// <param name="maxRadius1">Górny zakres długości promienia pionów pierwszego gracza</param>
        /// <param name="maxRadius2">Górny zakres długości promienia pionów drugiego gracza</param>
        /// <param name="size"></param>
        public Chessboard(Bgr minColorRange1, Bgr minColorRange2, Bgr maxColorRange1, Bgr maxColorRange2, double minDistance1, double minDistance2, int minRadius1, int minRadius2, int maxRadius1, int maxRadius2, int size = 10)
        {
            _size = size;
            _fields = new List<Field>();

            #region Uzupełnienie danych na temat pionów graczy
            PawnsInfo.minColorRange1 = minColorRange1;
            PawnsInfo.minColorRange2 = minColorRange2;
            PawnsInfo.maxColorRange1 = maxColorRange1;
            PawnsInfo.maxColorRange2 = maxColorRange2;
            PawnsInfo.minDistance1 = minDistance1;
            PawnsInfo.minDistance2 = minDistance2;
            PawnsInfo.minRadius1 = minRadius1;
            PawnsInfo.minRadius2 = minRadius2;
            PawnsInfo.maxRadius1 = maxRadius1;
            PawnsInfo.maxRadius2 = maxRadius2;
            #endregion
        }
    }
}