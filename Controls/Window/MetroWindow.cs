using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MultiBranchTexter
{
    public class MetroWindow : Window
    {
        #region 依赖属性
        public static DependencyProperty ShowSettingsProperty =
          DependencyProperty.Register("ShowSettings", typeof(bool),
              typeof(MetroWindow), new PropertyMetadata(false));

        public bool ShowSettings
        {
            get { return (bool)GetValue(ShowSettingsProperty); }
            set { SetValue(ShowSettingsProperty, value); }
        }
        #endregion

        //标题栏
        private CheckBox MaxButton;
        private TextBlock WindowTitleTbl;

        public MetroWindow()
        {
            this.Loaded += MetroWindow_Loaded;
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 查找窗体模板
            if (App.Current.Resources["MetroWindowTemplate"] is ControlTemplate mWTemplate)
            {
                MaxButton = mWTemplate.FindName("MaxWinButton", this) as CheckBox;

                MaxButton.Click += MaxButton_Click;
                (mWTemplate.FindName("SettingButton", this) as Button).Click += SettingBtn_Click;
                (mWTemplate.FindName("CloseWinButton", this) as Button).Click += CloseButton_Click;
                (mWTemplate.FindName("MinWinButton", this) as Button).Click += MinButton_Click;
                (mWTemplate.FindName("UpperBd", this) as Border).MouseDown += UpperBd_MouseDown; 

                WindowTitleTbl = mWTemplate.FindName("WindowTitleTbl", this) as TextBlock;
                WindowTitleTbl.Text = Title;
            }
        }

        private void UpperBd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowSettings = false;
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings = !ShowSettings;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Application.Current.MainWindow as MainWindow).GetFCC().IsModified == "*")
            {
                MessageBoxResult warnResult = MessageBox.Show
                    (
                    Application.Current.MainWindow,
                    "尚未保存，是否保存？",
                    "警告",
                    MessageBoxButton.YesNoCancel
                    );
                if (warnResult == MessageBoxResult.Yes)
                {
                    (Application.Current.MainWindow as MainWindow)
                        .ViewModel.SaveFile((Application.Current.MainWindow as MainWindow).GetFCC());
                    Close();
                }
                else if (warnResult == MessageBoxResult.No)
                {
                    Close();
                }
            }
            else { Close(); }
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaxButton.IsChecked == true)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 实现窗体移动
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
            base.OnMouseLeftButtonDown(e);
        }
    }
}
