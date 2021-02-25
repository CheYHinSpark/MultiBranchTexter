using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiBranchTexter.Controls
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
