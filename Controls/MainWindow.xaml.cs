using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void frontBtn_Click(object sender, RoutedEventArgs e)
        {
            flowChart.Visibility = Visibility.Visible;
        }

        //打开标签页
        public void OpenMBTabItem(TextNode node)
        {
            //遍历已有的标签页看看是否已经存在同name的标签
            foreach(MBTabItem mBTabItem in workTabControl.Items)
            {
                if (mBTabItem.textNode.Name == node.Name)
                {
                    workTabControl.SelectedItem = mBTabItem;
                    //隐藏flowChart
                    flowChart.Visibility = Visibility.Hidden;
                    return;
                }
            }
            workTabControl.Items.Add(new MBTabItem(node));
            workTabControl.SelectedIndex = workTabControl.Items.Count - 1;
            //隐藏flowChart
            flowChart.Visibility = Visibility.Hidden;
        }
    }
}
