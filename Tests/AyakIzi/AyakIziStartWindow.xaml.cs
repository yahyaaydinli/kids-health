using System.Windows;

namespace PsikologTestApp.Tests.AyakIzi
{
    public partial class AyakIziStartWindow : Window
    {
        public AyakIziStartWindow() { InitializeComponent(); }

        private void CountdownSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (CountdownLabel != null)
                CountdownLabel.Text = $"{(int)e.NewValue} saniye";
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            int gridSize = Grid4Radio.IsChecked  == true ? 4
                         : Grid9Radio.IsChecked  == true ? 9
                         : Grid16Radio.IsChecked == true ? 16
                         : 25;

            int countdown = (int)CountdownSlider.Value;
            new AyakIziWindow(gridSize, countdown).Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
