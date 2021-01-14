using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter
{
    public class MetroWindow : Window
    {
        private Button CloseButton;
        private CheckBox MaxButton;
        private Button MinButton;
        private TextBlock WindowTitleTbl;

        public MetroWindow()
        {
            this.Loaded += MetroWindow_Loaded;
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 7;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 查找窗体模板

            if (App.Current.Resources["MetroWindowTemplate"] is ControlTemplate metroWindowTemplate)
            {
                CloseButton = metroWindowTemplate.FindName("CloseWinButton", this) as Button;
                MaxButton = metroWindowTemplate.FindName("MaxWinButton", this) as CheckBox;
                MinButton = metroWindowTemplate.FindName("MinWinButton", this) as Button;

                CloseButton.Click += CloseButton_Click;
                MaxButton.Click += MaxButton_Click;
                MinButton.Click += MinButton_Click;

                WindowTitleTbl = metroWindowTemplate.FindName("WindowTitleTbl", this) as TextBlock;
                WindowTitleTbl.Text = Title;
            }
        }
      
        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
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

        private void MinButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        // 实现窗体移动
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
            base.OnMouseLeftButtonDown(e);
        }
    }
}
