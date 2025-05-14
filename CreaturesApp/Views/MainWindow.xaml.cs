using System.Windows;

namespace CreaturesApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); // Теперь компилятор сгенерирует этот метод
            DataContext = new ViewModels.MainViewModel();
        }
    }
}