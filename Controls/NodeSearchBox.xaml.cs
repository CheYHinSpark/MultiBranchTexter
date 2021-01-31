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
        private FlowChartContainer container;

        public NodeSearchBox()
        {
            InitializeComponent();
        }

        private void findBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //通知流程图容器搜索节点
            container.SearchNode(findBox.Text);
        }

        public void SetFlowChartContainer(FlowChartContainer newFCC)
        {
            container = newFCC;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            container.ClearSearch();
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            container.SearchNext(findBox.Text);
        }
    }
}
