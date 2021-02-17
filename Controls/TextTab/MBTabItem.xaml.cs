﻿using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.Controls
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
            tabEnd.SetTabEnd(node);
            _viewModel = DataContext as TabItemViewModel;
            _viewModel.NodeText = node.Text;
            _viewModel.IsModified = "";
        }

        #region 成员变量
        /// <summary>
        /// 相应的textNode
        /// </summary>
        public TextNode textNode;

        // 父级TabControl
        private TabControl parent;

        // 约定的宽度
        private readonly double conventionWidth = 120;

        private readonly TabItemViewModel _viewModel;
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
            Close();
        }

        /// <summary>
        /// 父级TabControl尺寸发生变化
        /// </summary>
        private void parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //调整自身大小
            //保持约定宽度item的临界个数
            int criticalCount = (int)(parent.ActualWidth / conventionWidth);
            if (parent.Items.Count <= criticalCount)
            {
                //小于等于临界个数 等于约定宽度
                this.Width = conventionWidth;
            }
            else
            {
                //大于临界个数 等于平均宽度
                double perWidth = parent.ActualWidth / parent.Items.Count;
                this.Width = perWidth;
            }
        }

        /// <summary>
        /// 按键
        /// </summary>
        private void TabItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.S)//保存
                {
                    textNode.Text = textBox.Text;
                }
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
            ObservableCollection<MBTabItem> parentItems = (parent.ItemsSource as ObservableCollection<MBTabItem>);

            //自适应
            //保持约定宽度item的临界个数
            int criticalCount = (int)(parent.ActualWidth / conventionWidth);
            if (parentItems.Count <= criticalCount)
            {
                //小于等于临界个数 等于约定宽度
                this.Width = conventionWidth;
            }
            else
            {
                //大于临界个数 每项都设成平均宽度
                double perWidth = parent.ActualWidth / parentItems.Count;
                foreach (MBTabItem item in parentItems)
                {
                    item.Width = perWidth;
                }
                this.Width = perWidth;
            }
        }
        #endregion

        /// <summary>
        /// 重置TabEnd
        /// </summary>
        public void ReLoadTabEnd()
        {
            tabEnd.LoadTabEnd();
        }

        /// <summary>
        /// 关闭自身
        /// </summary>
        public void Close()
        {
            if (parent == null)
            { return; }
            //检查保存
            if (_viewModel.IsModified == "*")
            {
                MessageBoxResult result = MessageBox.Show(
                    Application.Current.MainWindow,
                    "文本尚未保存，是否保存？",
                    "警告",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Cancel);
                if (result == MessageBoxResult.Cancel)
                { return; }
                else if (result == MessageBoxResult.Yes)
                { Save(); }
            }
            //移除自身
            ObservableCollection<MBTabItem> parentItems = (parent.ItemsSource as ObservableCollection<MBTabItem>);
            parentItems.Remove(this);
            //移除事件
            parent.SizeChanged -= parent_SizeChanged;

            //调整剩余项大小
            //保持约定宽度item的临界个数
            int criticalCount = (int)(parent.ActualWidth / conventionWidth);
            //平均宽度
            double perWidth = parent.ActualWidth / parentItems.Count;
            foreach (MBTabItem item in parentItems)
            {
                if (parentItems.Count <= criticalCount)
                {
                    item.Width = conventionWidth;
                }
                else
                {
                    item.Width = perWidth;
                }
            }
        }

        public void Save()
        {
            textNode.Text = _viewModel.NodeText;
            _viewModel.IsModified = "";
        }
        #endregion
    }
}