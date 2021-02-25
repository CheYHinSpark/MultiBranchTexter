using MultiBranchTexter.ViewModel;
using System.Collections.Generic;
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
            // 创建ViewModel全局映射
            Application.Current.Properties.Add("ViewModelMap",new Dictionary<string, object>());
            Startup += App_Startup;
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            this.MainWindow = window;
            if (e.Args.Length > 0)
            {
                ViewModelFactory.Main.OpenFile(e.Args[0]);
            }
            window.Show();
            ViewModelFactory.Settings.CheckUpdate();
        }
    }
}
