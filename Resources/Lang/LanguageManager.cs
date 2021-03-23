using MultiBranchTexter.Model;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace MultiBranchTexter.Resources
{
    /// <summary>
    /// 总之这是个找来的多语言管理器
    /// </summary>
    public class LanguageManager : INotifyPropertyChanged
    {
        private readonly ResourceManager _resourceManager;
        private static readonly Lazy<LanguageManager> _lazy = new Lazy<LanguageManager>(() => new LanguageManager());
        public static LanguageManager Instance => _lazy.Value;
        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageManager()
        { _resourceManager = new ResourceManager(typeof(Lang)); }

        public string this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                return _resourceManager.GetString(name);
            }
        }

        public void ChangeLanguage(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }

        /// <summary>
        /// 为了消灭一些bug必须预加载一次
        /// </summary>
        public void PreloadLanguage()
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
                ChangeLanguage(new CultureInfo("zh-CHS"));
            }
            else
            {
                ChangeLanguage(new CultureInfo("en"));
            }
        }
    }
}
