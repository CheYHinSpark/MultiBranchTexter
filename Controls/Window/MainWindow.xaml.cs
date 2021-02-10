using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
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
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = base.DataContext as MainViewModel;
        }

        #region 方法
        //打开标签页
        public void OpenMBTabItem(TextNode node)
        {
            _viewModel.OpenMBTabItem(node);
        }

      

        /// <summary>
        /// 返回首页，并且跳到对应节点位置
        /// </summary>
        public void BackToFront(TextNode node)
        {
            _viewModel.IsFlowChartShowing = true;
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
        #endregion
    }
}
