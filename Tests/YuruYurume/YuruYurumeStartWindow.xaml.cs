using System.Windows;
using System.Windows.Controls;
using PsikologTestApp;

namespace PsikologTestApp.Tests.YuruYurume
{
    public partial class YuruYurumeStartWindow : Window
    {
        public YuruYurumeStartWindow()
        {
            InitializeComponent();
        }

        private void GoPercentSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (GoPercentLabel == null) return;
            int go = (int)e.NewValue;
            GoPercentLabel.Text = $"{go}%";
            if (GoRatioHintLabel != null)
                GoRatioHintLabel.Text = $"(Yeşil: %{go}  —  Kırmızı: %{100 - go})";
        }

        private void DisplayIntervalSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (DisplayIntervalLabel != null)
                DisplayIntervalLabel.Text = $"{(int)e.NewValue} ms";
        }

        private void GapIntervalSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (GapIntervalLabel != null)
                GapIntervalLabel.Text = $"{(int)e.NewValue} ms";
        }

        private void DurationSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (DurationLabel != null)
                DurationLabel.Text = $"{(int)e.NewValue} dakika";
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            int duration  = (int)DurationSlider.Value;
            int goPercent = (int)GoPercentSlider.Value;
            int displayMs = (int)DisplayIntervalSlider.Value;
            int gapMs     = (int)GapIntervalSlider.Value;
            var testWindow = new YuruYurumeWindow(duration, goPercent, displayMs, gapMs);
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
