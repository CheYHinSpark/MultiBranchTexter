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
            this.Loaded += ModernWindow_Loaded;
        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
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
                this.WindowState = WindowState.Maximized;
                Left = 0;
                Top = 0;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void MinButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// 实现窗体移动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
            base.OnMouseLeftButtonDown(e);
        }
    }
}
