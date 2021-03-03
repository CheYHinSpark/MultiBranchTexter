using MultiBranchTexter.ViewModel;
using System;
using System.Windows;

namespace MultiBranchTexter
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

        private void BgGrid_Drop(object sender, DragEventArgs e)
        {
            string fileName =((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            ViewModelFactory.Main.OpenFile(fileName);
        }
    }
}
