using System;
using System.Windows;

namespace PsikologTestApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += (_, args) =>
            {
                MessageBox.Show(
                    $"Hata:\n{args.Exception.GetType().Name}\n\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                    "Beklenmeyen Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}
