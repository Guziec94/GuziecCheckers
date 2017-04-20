using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

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
                Chessboard szachownica = new Chessboard(10.0, 10, 20); 

                while (true)
                {
                    Mat matImage = Chessboard.kamera.QueryFrame();
                    Image<Bgr, byte> obraz = matImage.ToImage<Bgr, byte>();

                    szachownica.Calibration(obraz, true);

                    List<string> P1moves = szachownica.FindMoves(1);
                    List<string> P2moves = szachownica.FindMoves(2);

                    P1movesList.Dispatcher.Invoke(() => { P1movesList.Items.Clear(); });
                    P2movesList.Dispatcher.Invoke(() => { P2movesList.Items.Clear(); });

                    foreach (string move in P1moves) P1movesList.Dispatcher.Invoke(() => { P1movesList.Items.Add(move); });
                    foreach (string move in P2moves) P2movesList.Dispatcher.Invoke(() => { P2movesList.Items.Add(move); });

                    view.Dispatcher.Invoke(() => { view.Source = Tools.ImageToBitmapSource(obraz); });
                }
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
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
