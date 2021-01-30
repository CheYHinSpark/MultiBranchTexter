using MultiBranchTexter.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

        #region 事件
        //各种按键
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //Ctrl 配合
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                //Ctrl+A 全选
                if (e.Key == Key.A)
                { }
                //Ctrl+S 保存
                if (e.Key == Key.S)
                { }
                //Ctrl+O 打开
                if (e.Key == Key.O)
                { }
                //Ctrl+N 新建
                if (e.Key == Key.N)
                { }
                //Ctrl+F 寻找
                if (e.Key == Key.F)
                { }
                //Ctrl+H 替换
                if (e.Key == Key.H)
                { }
            }
            //Esc 退到首页
            if (e.Key == Key.Escape)
            { flowChart.Visibility = Visibility.Visible; }
        }

        private void frontBtn_Click(object sender, RoutedEventArgs e)
        {
            flowChart.Visibility = Visibility.Visible;
        }
        #endregion

        #region 方法
        //打开标签页
        public void OpenMBTabItem(TextNode node)
        {
            //遍历已有的标签页看看是否已经存在同name的标签
            foreach (MBTabItem mBTabItem in workTabControl.Items)
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

        //返回首页
        public void BackToFront()
        {
            flowChart.Visibility = Visibility.Visible;
        }
        #endregion
      
    }
}
