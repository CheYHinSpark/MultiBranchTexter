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
            // 相当于预加载一次语言，为了避免某些巴哥
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

        // worktab大小改变，通知mainviewmodel
        private void WorkTabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModelFactory.Main.WorkTabActualWidth = workTabControl.ActualWidth;
        }

        #region 拖入事件
        //拖入打开文件
        private void BgGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName =((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            ViewModelFactory.Main.OpenFile(fileName);
            baseGrid.Tag = null;
        }
        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            baseGrid.Tag = null;
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            baseGrid.Tag = "d";
        }
        #endregion


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            UpdateEffect();
        }
    }
}