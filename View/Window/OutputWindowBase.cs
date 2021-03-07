using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.View
{
    public class OutputWindowBase : Window
    {
        #region 依赖属性
        //保存路径
        public static DependencyProperty SavePathProperty =
            DependencyProperty.Register("SavePath", typeof(string),
              typeof(OutputWindowBase), new PropertyMetadata(""));

        public string SavePath
        {
            get { return (string)GetValue(SavePathProperty); }
            set { SetValue(SavePathProperty, value); }
        }

        //是否保存注释
        public static DependencyProperty SaveCommentProperty =
            DependencyProperty.Register("SaveComment", typeof(bool),
                typeof(OutputWindowBase), new PropertyMetadata(false));

        public bool SaveComment
        {
            get { return (bool)GetValue(SaveCommentProperty); }
            set { SetValue(SaveCommentProperty, value); }
        }

        //是否逐个保存
        public static DependencyProperty SaveIndividuallyProperty =
            DependencyProperty.Register("SaveIndividually", typeof(bool),
                typeof(OutputWindowBase), new PropertyMetadata(false));

        public bool SaveIndividually
        {
            get { return (bool)GetValue(SaveIndividuallyProperty); }
            set { SetValue(SaveIndividuallyProperty, value); }
        }
        #endregion

        protected string _fileFilter;

        public OutputWindowBase()
        {
            this.Loaded += OutputWindowBase_Loaded;
        }

        private void OutputWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            // 查找窗体模板
            if (App.Current.Resources["OutputWindowTemplate"] is ControlTemplate oWTemplate)
            {
                (oWTemplate.FindName("CloseWinButton", this) as Button).Click += CloseBtn_Click; ;
                (oWTemplate.FindName("MinWinButton", this) as Button).Click += MinButton_Click;
                (oWTemplate.FindName("BrowseButton", this) as Button).Click += BrowseBtn_Click;
                (oWTemplate.FindName("OutputButton", this) as Button).Click += OutputBtn_Click;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.WindowState = WindowState.Minimized;
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            // 文件夹对话框
            Microsoft.Win32.SaveFileDialog dialog =
                 new Microsoft.Win32.SaveFileDialog
                 {
                     RestoreDirectory = true,
                     Filter = _fileFilter
                 };
            string _fileDirPath = Path.GetDirectoryName(SavePath);
            if (Directory.Exists(_fileDirPath))
            { dialog.InitialDirectory = _fileDirPath; }
            if (dialog.ShowDialog() == true)
            {
                SavePath = dialog.FileName;
            }
        }

        private void OutputBtn_Click(object sender, RoutedEventArgs e)
        {
            Output();
        }

        protected virtual void Output()
        { }
    }
}
