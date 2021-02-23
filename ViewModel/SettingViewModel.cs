using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
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
            {
                _allowDoubleEnter = value;
                if (value == true)
                { (Application.Current.MainWindow as MainWindow).AllowDoubleEnter = true; }
                else
                { (Application.Current.MainWindow as MainWindow).AllowDoubleEnter = false; }
                RaisePropertyChanged("AllowDoubleEnter");
            }
        }

        public SettingViewModel()
        {
            ReadIni();
            ColorR = 238;
            ColorG = 170;
            ColorB = 22;
        }

        public void ChangeColor()
        {
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
            Debug.WriteLine("成功读取配置");
        }

        public void WriteIni()
        {
            IniFile iniFile = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "Settings.ini");
            iniFile.WriteBool("Settings", "IsDarkMode", IsDarkMode == true);
            iniFile.WriteBool("Settings", "AllowDoubleEnter", AllowDoubleEnter == true);
            Debug.WriteLine("成功保存配置");
        }
    }
}
