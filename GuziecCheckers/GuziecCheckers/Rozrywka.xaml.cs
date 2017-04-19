using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuziecCheckers
{
    /// <summary>
    /// Interaction logic for Rozrywka.xaml
    /// </summary>
    public partial class Rozrywka : Page
    {
        public static Thread t = null;

        #region Ciało wątku przetwarzającego obraz napływający z kamery
        private void w()
        {
            try
            {
                Chessboard szachownica = new Chessboard(10.0, 14, 18);

                Capture kamera = new Capture(0);
                while (true)
                {
                    Mat matImage = kamera.QueryFrame();
                    Image<Bgr, byte> obraz = matImage.ToImage<Bgr, byte>();

                    szachownica.Calibration(obraz, false, false);

                    view.Dispatcher.Invoke(() => { view.Source = Tools.ImageToBitmapSource(obraz); });
                }
            }
            catch (Exception) { }
        }
        #endregion

        public Rozrywka()
        {
            InitializeComponent();

            #region Uruchamiamy wątek przetwarzający obraz z kamery
            t = new Thread(w);
            t.Start();
            #endregion
        }
    }
}
