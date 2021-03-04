using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MultiBranchTexter.View;
using MultiBranchTexter.Model;

namespace MultiBranchTexter.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        #region 设置

        #region 勾选
        private bool _isDarkMode;

        public bool IsDarkMode
        {
            get { return _isDarkMode; }
            set
            {
                _isDarkMode = value;
                Application.Current.Resources.MergedDictionaries
                    .RemoveAt(Application.Current.Resources.MergedDictionaries.Count - 1);
                Application.Current.Resources.MergedDictionaries
                    .Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/Resources/" +
                        (value == true ? "Dark" : "Light") +
                        "ColorDictionary.xaml")
                    });
                RaisePropertyChanged("IsDarkMode");
            }
        }

        private bool _allowDoubleEnter;

        public bool AllowDoubleEnter
        {
            get { return _allowDoubleEnter; }
            set
            { _allowDoubleEnter = value; RaisePropertyChanged("AllowDoubleEnter"); }
        }

        private bool _countOpChar;

        public bool CountOpChar
        {
            get { return _countOpChar; }
            set
            {
                _countOpChar = value;
                foreach (MBTabItem tab in ViewModelFactory.Main.WorkTabs)
                { tab.ViewModel.CountCharWord(true); }
                RaisePropertyChanged("CountOpChar");
            }
        }
        #endregion

        #region 颜色
        private double _colorR;

        public double ColorR
        {
            get { return _colorR; }
            set 
            { 
                _colorR = value;
                ChangeColor();
                RaisePropertyChanged("ColorR");
            }
        }

        private double _colorG;

        public double ColorG
        {
            get { return _colorG; }
            set
            {
                _colorG = value;
                ChangeColor();
                RaisePropertyChanged("ColorG");
            }
        }

        private double _colorB;

        public double ColorB
        {
            get { return _colorB; }
            set
            {
                _colorB = value;
                ChangeColor();
                RaisePropertyChanged("ColorB");
            }
        }
        #endregion

        //文本编辑器两侧的宽度，可以用于避免文本区域过宽
        private double _sideWidth;
        public double SideWidth
        {
            get { return _sideWidth; }
            set
            { _sideWidth = value; RaisePropertyChanged("SideWidth"); }
        }

        //标题栏高度，用于全屏显示
        private double _titleBarHeight;
        public double TitleBarHeight
        {
            get { return _titleBarHeight; }
            set
            {
                _titleBarHeight = value;
                if (value == 0)
                { (Application.Current.MainWindow as MetroWindow).ToMaximize(); }
                RaisePropertyChanged("TitleBarHeight"); 
            }
        }
        #endregion

        #region 版本信息
        private string _versionInfo;
        public string VersionInfo
        {
            get { return _versionInfo; }
            set
            { _versionInfo = value; RaisePropertyChanged("VersionInfo"); }
        }

        private string _newVersionInfo;
        public string NewVersionInfo
        {
            get { return _newVersionInfo; }
            set
            { _newVersionInfo = value; RaisePropertyChanged("NewVersionInfo"); }
        }
        #endregion

        public SettingViewModel()
        {
            ReadIni();
            Version tempV = Application.ResourceAssembly.GetName().Version;
            //只要前三位
            VersionInfo = "当前版本  "
                + tempV.Major.ToString() + "." + tempV.Minor.ToString() + "." + tempV.Build.ToString();
            NewVersionInfo = "";
            TitleBarHeight = 24;
        }

        #region 命令
        // 还原默认设置命令
        public ICommand ToDefaultCommand => new RelayCommand((t) =>
        {
            IsDarkMode = false;
            AllowDoubleEnter = false;
            CountOpChar = false;
            _colorR = 238;
            _colorG = 170;
            ColorB = 22;
            SideWidth = 10;
            WriteIni();
        });

        // 打开项目主页命令
        public ICommand OpenMainWebCommand => new RelayCommand((t) =>
        {
            try
            {
                Process.Start("explorer.exe", "https://github.com/CheYHinSpark/MultiBranchTexter");
            }
            catch { }
        });

        // 打开项目说明命令
        public ICommand OpenIntroCommand => new RelayCommand((t) =>
        {
            try
            {
                Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory + "README.md");
            }
            catch { }
        });

        // 检查更新命令
        public ICommand CheckUpdateCommand => new RelayCommand(async (t) =>
        {
            if (await UpdateChecker.CheckGitHubNewerVersion())
            { DemandUpdate(); }
            else
            { MessageBox.Show("当前已是最新版本"); }
        });
        #endregion

        public async void CheckUpdate()
        {
            if (await UpdateChecker.CheckGitHubNewerVersion())
            { DemandUpdate(); }
        }

        public void DemandUpdate()
        {
            MessageBoxResult result = MessageBox
                  .Show("检测到新版本  " + NewVersionInfo
                  + "\n" + VersionInfo
                  + "\n是否前往更新？",
                  "检查更新",
                  MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            { Process.Start("explorer.exe", "https://github.com/CheYHinSpark/MultiBranchTexter/releases"); }
        }

        private void ChangeColor()
        {
            _colorR = Math.Max(0, Math.Min(255, _colorR));
            _colorG = Math.Max(0, Math.Min(255, _colorG));
            _colorB = Math.Max(0, Math.Min(255, _colorB));
            Color newColor = Color.FromRgb((byte)ColorR, (byte)ColorG, (byte)ColorB);
            Application.Current.Resources.MergedDictionaries[0]["Theme"] = newColor;
            Application.Current.Resources.MergedDictionaries[0]["ThemeBrush"] = new SolidColorBrush(newColor);
            IsDarkMode = IsDarkMode;// <--这是为了解决某些东西不会刷新
        }

        public void ReadIni()
        {
            IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
            IsDarkMode = iniFile.GetBool("Settings", "IsDarkMode", false);
            AllowDoubleEnter = iniFile.GetBool("Settings", "AllowDoubleEnter", false);
            CountOpChar = iniFile.GetBool("Settings", "CountOpChar", false);
            SideWidth = iniFile.GetInt("Settings", "SideWidth", 10);
            _colorR = iniFile.GetInt("Color", "Red", 238);
            _colorG = iniFile.GetInt("Color", "Green", 170);
            ColorB = iniFile.GetInt("Color", "Blue", 22);
            Debug.WriteLine("成功读取配置");
        }

        public void WriteIni()
        {
            IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
            iniFile.WriteBool("Settings", "IsDarkMode", IsDarkMode == true);
            iniFile.WriteBool("Settings", "AllowDoubleEnter", AllowDoubleEnter == true);
            iniFile.WriteBool("Settings", "CountOpChar", CountOpChar == true);
            iniFile.WriteInt("Settings", "SideWidth", (int)SideWidth);
            iniFile.WriteInt("Color", "Red", (int)_colorR);
            iniFile.WriteInt("Color", "Green", (int)_colorG);
            iniFile.WriteInt("Color", "Blue", (int)_colorB);
            Debug.WriteLine("成功保存配置");
        }
    }
}
