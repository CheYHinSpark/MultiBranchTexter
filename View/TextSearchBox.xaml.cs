using MultiBranchTexter.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// TextSearchBox.xaml 的交互逻辑
    /// </summary>
    public partial class TextSearchBox : UserControl
    {
        public TextSearchBox()
        {
            InitializeComponent();
        }

        private void FindBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (findBox.Text.Length < 1)
            { return; }
            //搜索开始
            ViewModelFactory.Main.WorkingViewModel.Search(findBox.Text);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        { ViewModelFactory.Main.WorkingViewModel.SearchBoxVisi = "0"; }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel.SearchNext();
        }

        private void LastBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel.SearchNext(true);
        }
    }
}
