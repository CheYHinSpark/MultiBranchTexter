using MultiBranchTexter.Model;
using MultiBranchTexter.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;

namespace MultiBranchTexter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
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
                    if (flowChart.Visibility == Visibility.Visible)
                    {
                        flowChart.searchBox.Visibility = Visibility.Visible;
                    }
                }
                //Ctrl+H 替换
                if (e.Key == Key.H)
                { }
            }
            //Esc
            if (e.Key == Key.Escape)
            {
                if (flowChart.Visibility == Visibility.Hidden)
                { flowChart.Visibility = Visibility.Visible; }
                else if (workTabControl.Items.Count > 0)//可以切换回标签们
                { flowChart.Visibility = Visibility.Hidden; }
            }
        }

        private void frontBtn_Click(object sender, RoutedEventArgs e)
        {
            flowChart.Visibility = Visibility.Visible;
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

        /// <summary>
        /// 返回首页
        /// </summary>
        public void BackToFront()
        {
            flowChart.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 返回首页，并且跳到对应节点位置
        /// </summary>
        public void BackToFront(TextNode node)
        {
            flowChart.Visibility = Visibility.Visible;
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
                flowChart.Visibility = Visibility.Visible;
                workTabControl.Visibility = Visibility.Visible;
            }
        }
        #endregion
    }
}
