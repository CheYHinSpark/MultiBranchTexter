using MultiBranchTexter.Model;
using MultiBranchTexter.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.ComponentModel;

namespace MultiBranchTexter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region 依赖属性
        public static DependencyProperty FlowChartWidthProperty =
          DependencyProperty.Register("FlowChartWidth", typeof(string),
              typeof(MainWindow), new PropertyMetadata("*"));

        public string FlowChartWidth
        {
            get { return (string)GetValue(FlowChartWidthProperty); }
            set 
            {
                SetValue(FlowChartWidthProperty, value);
                if (value == "*")
                { flowChartBtn.IsChecked = true; }
                else
                { flowChartBtn.IsChecked = false; }
                flowChartBtn.IsEnabled = true;
                workTabBtn.IsEnabled = true;
                if (workTabBtn.IsChecked != true)
                { flowChartBtn.IsEnabled = false; }
                else if (flowChartBtn.IsChecked != true)
                { workTabBtn.IsEnabled = false; }
            }
        }

        public static DependencyProperty WorkTabWidthProperty =
          DependencyProperty.Register("WorkTabWidth", typeof(string),
              typeof(MainWindow), new PropertyMetadata("0"));

        public string WorkTabWidth
        {
            get { return (string)GetValue(WorkTabWidthProperty); }
            set 
            {
                SetValue(WorkTabWidthProperty, value);
                if (value == "*")
                { workTabBtn.IsChecked = true; }
                else
                { workTabBtn.IsChecked = false; }
                flowChartBtn.IsEnabled = true;
                workTabBtn.IsEnabled = true;
                if (flowChartBtn.IsChecked != true)
                { workTabBtn.IsEnabled = false; }
                else if (workTabBtn.IsChecked != true)
                { flowChartBtn.IsEnabled = false; }
            }
        }
        #endregion


        private int tabFontSize = 14;
        public int TabFontSize
        {
            get { return tabFontSize; }
            set 
            { 
                tabFontSize = value; 
                if (tabFontSize < 6)
                { tabFontSize = 6; }
                else if (tabFontSize > 36)
                { tabFontSize = 36; }
                foreach (MBTabItem item in workTabControl.Items)
                { item.SetFontSize(tabFontSize); }
            }
        }

        public string FileDirPath = "";//文件夹位置


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
                { OpenFile(); }
                //Ctrl+N 新建
                if (e.Key == Key.N)
                { }
                //Ctrl+F 寻找
                if (e.Key == Key.F)
                {
                    if (FlowChartWidth == "*")
                    {
                        flowChart.searchBox.Visibility = Visibility.Visible;
                    }
                }
                //Ctrl+H 替换
                if (e.Key == Key.H)
                { }
            }
        }

        //这是控制显示模式的
        private void frontBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = (sender as CheckBox).Name;
            if (name == "flowChartBtn")//点击了节点图
            {
                FlowChartWidth = flowChartBtn.IsChecked == true ? "*" : "0";
            }
            else
            {
                WorkTabWidth = workTabBtn.IsChecked == true ? "*" : "0";
                
            }
        }

        private void FontBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "fontDownBtn")
            { TabFontSize--; }
            else
            { TabFontSize++; }
        }

        private void fileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        #endregion

        #region 方法
        //打开标签页
        public void OpenMBTabItem(TextNode node)
        {
            //遍历已有的标签页看看是否已经存在同标签
            foreach (MBTabItem mBTabItem in workTabControl.Items)
            {
                if (mBTabItem.textNode == node)
                {
                    workTabControl.SelectedItem = mBTabItem;
                    //打开worktab
                    WorkTabWidth = "*";
                    return;
                }
            }
            workTabControl.Items.Add(new MBTabItem(node));
            workTabControl.SelectedIndex = workTabControl.Items.Count - 1;
            //打开worktab
            WorkTabWidth = "*";
        }

        /// <summary>
        /// 返回首页
        /// </summary>
        public void CloseWorkTab()
        {
            FlowChartWidth = "*";
            WorkTabWidth = "0";
        }

        /// <summary>
        /// 返回首页，并且跳到对应节点位置
        /// </summary>
        public void BackToFront(TextNode node)
        {
            FlowChartWidth = "*";
            flowChart.ScrollToNode(node);
        }

        /// <summary>
        /// 重置某个标签页的页尾
        /// </summary>
        public void ReLoadTab(TextNode node)
        {
            foreach (MBTabItem item in workTabControl.Items)
            {
                if (item.textNode == node)
                {
                    item.ReLoadTabEnd();
                    return;
                }
            }
        }

        public void DeleteTab(TextNode node)
        {
            MBTabItem theItem = null;
            foreach (MBTabItem item in workTabControl.Items)
            {
                if (item.textNode == node)
                {
                    theItem = item;
                    break;
                }
            }
            theItem.Close();
        }

        private void OpenFile()
        {
            // 文件夹对话框
            Microsoft.Win32.OpenFileDialog dialog =
                new Microsoft.Win32.OpenFileDialog
                {
                    RestoreDirectory = true,
                    Filter = "多分支导航文件|*.mbtxt"
                };
            if (Directory.Exists(FileDirPath))
            {
                dialog.InitialDirectory = FileDirPath;
            }
            if (dialog.ShowDialog() == true)
            {
                //TODO: 检查是否需要保存现有文件

                fileNameTxt.Text = dialog.FileName;
                //关闭原有标签页
                while (workTabControl.Items.Count > 0)
                {
                    (workTabControl.SelectedItem as MBTabItem).Close();
                }
                //打开新文件
                flowChart.Load(dialog.FileName);
                workGrid.Visibility = Visibility.Visible;
            }
        }
        #endregion
    }
}
