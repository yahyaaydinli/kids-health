using System.Windows;
using System.Windows.Controls;
using PsikologTestApp;

namespace PsikologTestApp.Tests.RenkliKelimeler
{
    public partial class RenkliKelimelerStartWindow : Window
    {
        public RenkliKelimelerStartWindow()
        {
            InitializeComponent();
        }

        private void DurationSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (DurationLabel != null)
                DurationLabel.Text = $"{(int)e.NewValue} dakika";
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            int duration = (int)DurationSlider.Value;
            var testWindow = new RenkliKelimelerWindow(duration);
            testWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
