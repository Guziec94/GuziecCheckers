﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Navigation;
using static GuziecCheckers.Chessboard;

namespace GuziecCheckers
{
    /// <summary>
    /// Interaction logic for Rozrywka.xaml
    /// </summary>
    public partial class Rozgrywka : Page
    {
        public static Thread t = null;

        private Chessboard szachownica = null;
        private string _move = null;

        #region Ciało wątku przetwarzającego obraz napływający z kamery
        private void w()
        {
            szachownica = new Chessboard();

            while (true)
            {
                try
                {
                    Mat matImage = Chessboard.kamera.QueryFrame();
                    Image<Bgr, byte> obraz = matImage.ToImage<Bgr, byte>();

                    bool showChecked = false;
                    showLinesCheckBox.Dispatcher.Invoke(() => {
                        if ((bool)showLinesCheckBox.IsChecked) { showChecked = true; }
                    });

                    bool calibrated = szachownica.Calibration(obraz, showChecked);

                    if (calibrated)
                    {
                        bool exist = false;

                        List<string> P1moves = szachownica.FindMoves(1);

                        P1movesList.Dispatcher.Invoke(() => {
                            List<string> old = P1movesList.Items.Cast<string>().ToList();

                            if (!P1moves.SequenceEqual(old))
                            {
                                P1movesList.Items.Clear();
                                foreach (string move in P1moves) P1movesList.Items.Add(move);
                            }
                            if (_move != null && P1movesList.Items.Contains(_move)) exist = true;
                        });

                        List<string> P2moves = szachownica.FindMoves(2);

                        P2movesList.Dispatcher.Invoke(() => {
                            List<string> old = P2movesList.Items.Cast<string>().ToList();

                            if (!P2moves.SequenceEqual(old))
                            {
                                P2movesList.Items.Clear();
                                foreach (string move in P2moves) P2movesList.Items.Add(move);
                            }
                            if (_move != null && P2movesList.Items.Contains(_move)) exist = true;
                        });

                        if (exist) showMove(ref obraz, _move);
                    }
                    else
                    {
                        P1movesList.Dispatcher.Invoke(() => { P1movesList.Items.Clear(); });
                        P2movesList.Dispatcher.Invoke(() => { P2movesList.Items.Clear(); });
                    }

                    view.Dispatcher.Invoke(() => { view.Source = Tools.ImageToBitmapSource(obraz); });
                }
                catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }  
            }
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

        private void P1movesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _move = P1movesList.SelectedValue.ToString();
                hideMove.IsEnabled = true;
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }

        private void P2movesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _move = P2movesList.SelectedValue.ToString();
                hideMove.IsEnabled = true;
            }
            catch (Exception /*ex*/) { /*System.Windows.MessageBox.Show(ex.Message);*/ }
        }
    }
}
