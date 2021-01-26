using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter
{
    /// <summary>
    /// MBTabItem.xaml 的交互逻辑
    /// </summary>
    public partial class MBTabItem : TabItem
    {
        public MBTabItem(TextNode node)
        {
            InitializeComponent();
            textNode = node;
            Header = node.Name;
            textBox.Text = node.Text;
        }

        #region 成员变量
        /// <summary>
        /// 相应的textNode
        /// </summary>
        public TextNode textNode;
        /// <summary>
        /// 父级TabControl
        /// </summary>
        private TabControl parent;
        /// <summary>
        /// 约定的宽度
        /// </summary>
        private double conventionWidth = 120;
        #endregion

        #region 事件
        /// <summary>
        /// loaded
        /// </summary>
        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            //找到父级TabControl
            parent = ControlTreeHelper.FindParentOfType<TabControl>(this);
            // 查找控件模板
            if (App.Current.Resources["MBTabItemTemplate"] is ControlTemplate tabItemTemplate)
            {
                (tabItemTemplate.FindName("CloseBtn", this) as Button).Click += CloseBtn_Click;
            }
            if (parent != null)
            { Load(); }
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (parent == null)
            { return; }

            //移除自身
            parent.Items.Remove(this);
            //移除事件
            parent.SizeChanged -= parent_SizeChanged;

            //调整剩余项大小
            //保持约定宽度item的临界个数
            int criticalCount = (int)((parent.ActualWidth - 5) / conventionWidth);
            //平均宽度
            double perWidth = (parent.ActualWidth - 5) / parent.Items.Count;
            foreach (MBTabItem item in parent.Items)
            {
                if (parent.Items.Count <= criticalCount)
                {
                    item.Width = conventionWidth;
                }
                else
                {
                    item.Width = perWidth;
                }
            }
            //如果是最后一项被关掉了
            if (parent.Items.Count == 0)
            {
                // 本控件已经移除，所以依赖对象不能是this
                ControlTreeHelper.FindParentOfType<MainWindow>(parent).BackToFront();
            }
        }
        /// <summary>
        /// 父级TabControl尺寸发生变化
        /// </summary>
        private void parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //调整自身大小
            //保持约定宽度item的临界个数
            int criticalCount = (int)((parent.ActualWidth - 5) / conventionWidth);
            if (parent.Items.Count <= criticalCount)
            {
                //小于等于临界个数 等于约定宽度
                this.Width = conventionWidth;
            }
            else
            {
                //大于临界个数 等于平均宽度
                double perWidth = (parent.ActualWidth - 5) / parent.Items.Count;
                this.Width = perWidth;
            }
        }
        #endregion

        #region 方法
        #region Load
        /// <summary>
        /// Load
        /// </summary>
        private void Load()
        {
            //注册父级TabControl尺寸发生变化事件
            parent.SizeChanged += parent_SizeChanged;

            //自适应
            //保持约定宽度item的临界个数
            int criticalCount = (int)((parent.ActualWidth - 5) / conventionWidth);
            if (parent.Items.Count <= criticalCount)
            {
                //小于等于临界个数 等于约定宽度
                this.Width = conventionWidth;
            }
            else
            {
                //大于临界个数 每项都设成平均宽度
                double perWidth = (parent.ActualWidth - 5) / parent.Items.Count;
                foreach (MBTabItem item in parent.Items)
                {
                    item.Width = perWidth;
                }
                this.Width = perWidth;
            }
        }
        #endregion

        /// <summary>
        /// 设置字体大小
        /// </summary>
        public void SetFontSize(int newSize)
        {
            textBox.FontSize = newSize;
        }
        #endregion
    }
}
