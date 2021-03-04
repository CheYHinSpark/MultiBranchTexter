using MultiBranchTexter.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// NodeSearchBox.xaml 的交互逻辑
    /// </summary>
    public partial class NodeSearchBox : UserControl
    {
        public NodeSearchBox()
        {
            InitializeComponent();
        }

        private void FindBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //通知流程图容器搜索节点
            ViewModelFactory.FCC.SearchNode(findBox.Text);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            ViewModelFactory.FCC.ClearSearch();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.FCC.SearchNext(findBox.Text);
        }
    }
}
