using System.Linq;
using System.Windows;
using PsikologTestApp.Tests.DogruHedef.Models;

namespace PsikologTestApp.Tests.DogruHedef
{
    public partial class DogruHedefStartWindow : Window
    {
        public DogruHedefStartWindow() { InitializeComponent(); }

        private void DurationSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (DurationLabel != null)
                DurationLabel.Text = $"{(int)e.NewValue} dakika";
        }

        private void CountdownSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (CountdownLabel != null)
                CountdownLabel.Text = $"{(int)e.NewValue} saniye";
        }

        private void MinMsSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (MinMsLabel == null) return;
            int v = (int)e.NewValue;
            MinMsLabel.Text = $"{v} ms";
            if (MaxMsSlider != null && v > (int)MaxMsSlider.Value)
                MaxMsSlider.Value = v;
        }

        private void MaxMsSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (MaxMsLabel == null) return;
            int v = (int)e.NewValue;
            MaxMsLabel.Text = $"{v} ms";
            if (MinMsSlider != null && v < (int)MinMsSlider.Value)
                MinMsSlider.Value = v;
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            string selectedName =
                AnimalKediRadio.IsChecked    == true ? "Kedi"    :
                AnimalKopekRadio.IsChecked   == true ? "Köpek"   :
                AnimalKurbagaRadio.IsChecked == true ? "Kurbağa" :
                AnimalTavsanRadio.IsChecked  == true ? "Tavşan"  :
                "Tilki";

            var animal    = DogruHedefAnimal.All.First(a => a.Name == selectedName);
            int duration  = (int)DurationSlider.Value;
            int countdown = (int)CountdownSlider.Value;
            int minMs     = (int)MinMsSlider.Value;
            int maxMs     = (int)MaxMsSlider.Value;

            new DogruHedefWindow(animal, duration, countdown, minMs, maxMs).Show();
            this.Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
