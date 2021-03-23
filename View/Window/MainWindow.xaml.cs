using MultiBranchTexter.Model;
using MultiBranchTexter.Resources;
using MultiBranchTexter.ViewModel;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            string culInfo;
            if (CultureInfo.CurrentCulture.Name[..2] == "zh")
            { culInfo = "zh-CHS"; }
            else
            { culInfo = "en"; }

            IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");

            culInfo = iniFile.GetString("Language", "Language", culInfo);
            if (culInfo == "zh-CHS")
            {
                ViewModelFactory.Settings.LangIndex = 0;
                LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-CHS"));
            }
            else
            {
                ViewModelFactory.Settings.LangIndex = 1;
                LanguageManager.Instance.ChangeLanguage(new CultureInfo("en"));
            }


            InitializeComponent();
            ViewModelFactory.SetViewModel(typeof(MainViewModel), DataContext as MainViewModel);
        }

        //拖入打开文件
        private void BgGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName =((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            ViewModelFactory.Main.OpenFile(fileName);
        }

        //worktab大小改变，通知mainviewmodel
        private void WorkTabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModelFactory.Main.WorkTabActualWidth = workTabControl.ActualWidth;
        }
    }
}
