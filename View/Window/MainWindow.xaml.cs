using MultiBranchTexter.ViewModel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MultiBranchTexter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        //private Task _workTabTask;

        public MainWindow()
        {
            InitializeComponent();
            ViewModelFactory.SetViewModel(typeof(MainViewModel), DataContext as MainViewModel);
        }

        private void BgGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName =((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            ViewModelFactory.Main.OpenFile(fileName);
        }

        private void WorkTabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModelFactory.Main.WorkTabActualWidth = workTabControl.ActualWidth;
            ////调整自身大小
            ////保持约定宽度item的临界个数
            //int criticalCount = (int)(workTabControl.ActualWidth / 120.0);
            //double targetWidth;
            //if (workTabControl.Items.Count <= criticalCount)
            //{
            //    //小于等于临界个数 等于约定宽度
            //    targetWidth = 120;
            //}
            //else
            //{
            //    //大于临界个数 等于平均宽度
            //    targetWidth = workTabControl.ActualWidth / workTabControl.Items.Count;
            //}
            //for (int i =0; i<ViewModelFactory.Main.WorkTabs.Count;i++)
            //{
            //    ViewModelFactory.Main.WorkTabs[i].Width = targetWidth;
            //}
        }
    }
}
