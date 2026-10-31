using System.Windows;
using PsikologTestApp.Tests.AyakIzi;
using PsikologTestApp.Tests.Dedektif;
using PsikologTestApp.Tests.DogruHedef;
using PsikologTestApp.Tests.HedefAvcisi;
using PsikologTestApp.Tests.RenkliKelimeler;
using PsikologTestApp.Tests.YuruYurume;

namespace PsikologTestApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HedefAvcisiButton_Click(object sender, RoutedEventArgs e)
        {
            HedefAvcisiStartWindow startWindow = new HedefAvcisiStartWindow();
            startWindow.Show();
            this.Close();
        }

        private void RenkliKelimelerButton_Click(object sender, RoutedEventArgs e)
        {
            var startWindow = new RenkliKelimelerStartWindow();
            startWindow.Show();
            this.Close();
        }

        private void YuruYurumeButton_Click(object sender, RoutedEventArgs e)
        {
            var startWindow = new YuruYurumeStartWindow();
            startWindow.Show();
            this.Close();
        }

        private void DedektifButton_Click(object sender, RoutedEventArgs e)
        {
            var startWindow = new DedektifStartWindow();
            startWindow.Show();
            this.Close();
        }

        private void AyakIziButton_Click(object sender, RoutedEventArgs e)
        {
            new AyakIziStartWindow().Show();
            this.Close();
        }

        private void DogruHedefButton_Click(object sender, RoutedEventArgs e)
        {
            new DogruHedefStartWindow().Show();
            this.Close();
        }
    }
}
