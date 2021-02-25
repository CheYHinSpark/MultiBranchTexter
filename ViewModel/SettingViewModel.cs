using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MultiBranchTexter.Model;

namespace MultiBranchTexter.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private bool? _isDarkMode;

        public bool? IsDarkMode
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

        private bool? _allowDoubleEnter;

        public bool? AllowDoubleEnter
        {
            get { return _allowDoubleEnter; }
            set
            { _allowDoubleEnter = value; RaisePropertyChanged("AllowDoubleEnter"); }
        }

        public SettingViewModel()
        {
            ReadIni();
        }

        #region 命令
        // 还原默认设置命令
        public ICommand ToDefaultCommand => new RelayCommand((t) =>
        {
            IsDarkMode = false;
            AllowDoubleEnter = false;
            _colorR = 238;
            _colorG = 170;
            ColorB = 22;
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

        // 打开项目说明命令
        public ICommand CheckUpdateCommand => new RelayCommand((t) =>
        { CheckUpdate(); });
        #endregion

        public async void CheckUpdate()
        {
            await UpdateChecker.CheckGitHubNewerVersion();
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
            iniFile.WriteInt("Color", "Red", (int)_colorR);
            iniFile.WriteInt("Color", "Green", (int)_colorG);
            iniFile.WriteInt("Color", "Blue", (int)_colorB);
            Debug.WriteLine("成功保存配置");
        }
    }
}
