using MultiBranchTexter.ViewModel;
using System;
using System.Diagnostics;
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
