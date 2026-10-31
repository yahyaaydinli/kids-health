using System.Windows;

namespace PsikologTestApp.Tests.Dedektif
{
    public partial class DedektifStartWindow : Window
    {
        public DedektifStartWindow()
        {
            InitializeComponent();
        }

        private void FigureCountSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (FigureCountLabel != null)
                FigureCountLabel.Text = $"{(int)e.NewValue} figür";
        }

        private void CountdownSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (CountdownLabel != null)
                CountdownLabel.Text = $"{(int)e.NewValue} saniye";
        }

        private void DurationSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (DurationLabel != null)
                DurationLabel.Text = $"{(int)e.NewValue} dakika";
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            int figureCount   = (int)FigureCountSlider.Value;
            int countdownSecs = (int)CountdownSlider.Value;
            int duration      = (int)DurationSlider.Value;
            var testWindow    = new DedektifWindow(duration, figureCount, countdownSecs);
            testWindow.Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
