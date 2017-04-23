using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;

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
                while (true)
                {
                    Mat matImage = Chessboard.kamera.QueryFrame();

                    Image<Bgr, byte> obraz = matImage.ToImage<Bgr, byte>();

                    Image<Gray, byte> gray1 = obraz.InRange(Chessboard.PawnsInfo.minColorRange1, Chessboard.PawnsInfo.maxColorRange1);
                    Image<Gray, byte> gray2 = obraz.InRange(Chessboard.PawnsInfo.minColorRange2, Chessboard.PawnsInfo.maxColorRange2);

                    view1.Dispatcher.Invoke(() => { view1.Source = Tools.ImageToBitmapSource(gray1); });
                    view2.Dispatcher.Invoke(() => { view2.Source = Tools.ImageToBitmapSource(gray2); });
                }
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }
        #endregion

        public Kalibracja()
        {
            InitializeComponent();

            PxColor_TextChanged(null, null);
            #region Ustawiamy podpowiedzi
            P1Rmin.ToolTip = P1Gmin.ToolTip = P1Bmin.ToolTip = P2Rmin.ToolTip = P2Gmin.ToolTip = P2Bmin.ToolTip = "Minimum (0-255)";
            P1Rmax.ToolTip = P1Gmax.ToolTip = P1Bmax.ToolTip = P2Rmax.ToolTip = P2Gmax.ToolTip = P2Bmax.ToolTip = "Maksimum (0-255)";
            #endregion 
            #region Uruchamiamy wątek przetwarzający obraz z kamery
            t = new Thread(w);
            t.Start();
            #endregion
        }

        /// <summary>
        /// Event wywoływany w momencie aktualizacji danych RGB w polach podczas kalibracji
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PxColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Chessboard.PawnsInfo.minColorRange1 = new Bgr(Convert.ToInt32(P1Rmin.Text), Convert.ToInt32(P1Gmin.Text), Convert.ToInt32(P1Bmin.Text));
                Chessboard.PawnsInfo.maxColorRange1 = new Bgr(Convert.ToInt32(P1Rmax.Text), Convert.ToInt32(P1Gmax.Text), Convert.ToInt32(P1Bmax.Text));
                Chessboard.PawnsInfo.minColorRange2 = new Bgr(Convert.ToInt32(P2Rmin.Text), Convert.ToInt32(P2Gmin.Text), Convert.ToInt32(P2Bmin.Text));
                Chessboard.PawnsInfo.maxColorRange2 = new Bgr(Convert.ToInt32(P2Rmax.Text), Convert.ToInt32(P2Gmax.Text), Convert.ToInt32(P2Bmax.Text));
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }

        /// <summary>
        /// Event wywoływany po kliknięciu przycisku zapisu konfiguracji - Przechodzimy do panelu rozgrywki
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                t.Abort();

                Rozgrywka rozrywka = new Rozgrywka();
                NavigationService nav = NavigationService.GetNavigationService(this);
                nav.Navigate(rozrywka);
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }
    }
}