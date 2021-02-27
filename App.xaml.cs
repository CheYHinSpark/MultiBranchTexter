using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
            Current.Properties.Add("ViewModelMap",new Dictionary<string, object>());
            Startup += App_Startup;
        }

        async void App_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            this.MainWindow = window;
            if (e.Args.Length > 0)
            {
                ViewModelFactory.Main.OpenFile(e.Args[0]);
            }
            window.Show();
            //注册文件关联
            await Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    if (FileTypeRegister.FileTypeRegistered(".mbjson") == true)
                    {
                        return;
                    }
                    try
                    {
                        string culInfo = Thread.CurrentThread.CurrentCulture.TextInfo.CultureName;
                        int i = culInfo.IndexOf('-');
                        if (i >= 0)
                        {
                            culInfo = culInfo[0..i];
                            Debug.WriteLine(culInfo);
                        }

                        FileTypeRegInfo fileTypeRegInfo = new FileTypeRegInfo(".mbjson")
                        {
                            Description = "MBJSON " + culInfo == "zh" ? "文件" : "File",
                            ExePath = Process.GetCurrentProcess().MainModule.FileName,
                            ExtendName = ".mbjson",
                            IconPath = AppDomain.CurrentDomain.BaseDirectory + "Resources\\Images\\icon.ico"
                        };
                        FileTypeRegister.RegisterFileType(fileTypeRegInfo);
                    }
                    catch
                    {
#if !DEBUG
                        MessageBox.Show("初次启动，需要修改注册表，\n请重新以管理员身份启动一次程序。");
#endif
                    }
                }));
            await System.Threading.Tasks.Task.Delay(1000);// 这是为了让动画流畅走完
            //检查更新
            await Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    ViewModelFactory.Settings.CheckUpdate();
                }));
        }
    }
}
