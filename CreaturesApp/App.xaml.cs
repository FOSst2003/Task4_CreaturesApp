using System.Windows;

namespace CreaturesApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = new Views.MainWindow();
            window.DataContext = new ViewModels.MainViewModel();
            window.Show();
        }
    }
}