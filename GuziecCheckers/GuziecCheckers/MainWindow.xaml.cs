using System.Windows;

namespace GuziecCheckers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (Kalibracja.t != null && Kalibracja.t.IsAlive) Kalibracja.t.Abort();
        }
    }
}