using System.Windows;

namespace MultiBranchTexter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += App_Startup;
        }


        void App_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            this.MainWindow = window;
            if (e.Args.Length > 0)
            {
                window.ViewModel.OpenFile(e.Args[0]);
            }
            window.Show();
        }
    }
}
