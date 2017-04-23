using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GuziecCheckers.Chessboard;

namespace GuziecCheckers
{
    /// <summary>
    /// Interaction logic for Rozrywka.xaml
    /// </summary>
    public partial class Rozgrywka : Page
    {
        public static Thread t = null;
        public Chessboard szachownica = null;
        public Image<Bgr, byte> obraz = null;

        private string _move = null;

        #region Ciało wątku przetwarzającego obraz napływający z kamery
        private void w()
        {
            try
            {
                szachownica = new Chessboard();

                while (true)
                {
                    Mat matImage = Chessboard.kamera.QueryFrame();
                    obraz = matImage.ToImage<Bgr, byte>();

                    bool showChecked = false;
                    showLinesCheckBox.Dispatcher.Invoke(() => {
                        if ((bool)showLinesCheckBox.IsChecked) { showChecked = true; }
                    });

                    szachownica.Calibration(obraz, showChecked);

                    if (_move != null) showMove(ref obraz, _move);

                    List<string> P1moves = szachownica.FindMoves(1);
                    P1movesList.Dispatcher.Invoke(() => {
                        P1movesList.Items.Clear();
                        foreach (string move in P1moves) P1movesList.Items.Add(move);
                    });

                    List<string> P2moves = szachownica.FindMoves(2);
                    P2movesList.Dispatcher.Invoke(() => {
                        P2movesList.Items.Clear();
                        foreach (string move in P2moves) P2movesList.Items.Add(move);
                    });

                    view.Dispatcher.Invoke(() => { view.Source = Tools.ImageToBitmapSource(obraz); });
                }
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }
        #endregion

        public Rozgrywka()
        {
            InitializeComponent();

            #region Uruchamiamy wątek przetwarzający obraz z kamery
            t = new Thread(w);
            t.Start();
            #endregion    
        }

        private void button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                t.Abort();

                Kalibracja kalibracja = new Kalibracja();
                NavigationService nav = NavigationService.GetNavigationService(this);
                nav.Navigate(kalibracja);
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }

        private void showMove(ref Image<Bgr, byte> img, string move)
        {
            try
            {
                string[] fields = move.Split(' ');
                foreach (string field in fields)
                {
                    Field square = szachownica._fields.Find(f => f.column.ToString() == field[0].ToString() && f.row.ToString() == field[1].ToString());

                    Point[] points = new Point[] {

                    new Point(square.leftUp.X, square.leftUp.Y),
                    new Point(square.rightUp.X, square.rightUp.Y),
                    new Point(square.leftDown.X, square.leftDown.Y),
                    new Point(square.rightDown.X, square.rightDown.Y)
                    };

                    img.DrawPolyline(points, true, new Bgr(0, 255, 0), 2);
                }
            }
            catch (Exception /*ex*/)
            { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }

        private void hideMove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _move = null;
            hideMove.IsEnabled = false;
        }

        private void PmovesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _move = P1movesList.SelectedValue.ToString();
                hideMove.IsEnabled = true;
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }
    }
}
