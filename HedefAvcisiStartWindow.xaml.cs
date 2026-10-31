using System;
using System.Windows;
using System.Windows.Controls;

namespace PsikologTestApp
{
    public partial class HedefAvcisiStartWindow : Window
    {
        public HedefAvcisiStartWindow()
        {
            InitializeComponent();
        }

        private void DisplayIntervalSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (IntervalValueLabel != null)
                IntervalValueLabel.Text = $"{(int)e.NewValue} ms";
        }

        private void GapIntervalSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (GapValueLabel != null)
                GapValueLabel.Text = $"{(int)e.NewValue} ms";
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int durationMinutes))
            {
                int displayIntervalMs = Math.Max(100, (int)DisplayIntervalSlider.Value);
                int gapIntervalMs = (int)GapIntervalSlider.Value;
                HedefAvcisiWindow testWindow = new HedefAvcisiWindow(durationMinutes, displayIntervalMs, gapIntervalMs);
                testWindow.Show();
                this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
