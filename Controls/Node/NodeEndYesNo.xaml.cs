using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MultiBranchTexter.Model;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// NodeEndYesNo.xaml 的交互逻辑
    /// </summary>
    public partial class NodeEndYesNo : NodeBase
    {
        public NodeEndYesNo()
        {
            InitializeComponent();
        }

        public NodeEndYesNo(YesNoCondition yesNoCond)
        {
            InitializeComponent();
            titleBox.Text = yesNoCond.Question;
        }

        #region 事件
        //加载完成
        private void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            //设置显示顺序为2，以显示在connectingline上面
            Panel.SetZIndex(this, 2);
        }

        //双击标题，可以改标题
        private void titleBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            titleBox.Focusable = true;
        }

        //标题失去焦点，恢复为不可聚焦并完成标题修改
        private void titleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            titleBox.Focusable = false;
            // 完成问题修改
            fatherNode.textNode.endCondition.Question = titleBox.Text;
            titleBox.SelectionStart = 0;
            // 还要通知窗口改变相应的标签页
            ControlTreeHelper.FindParentOfType<MainWindow>(fatherNode).ReLoadTab(fatherNode.textNode);
        }

        //虽然0个引用，但这是双击两个小节点启动调整后继功能
        private void yesnoNode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NodeBase node = sender as NodeBase;
            FlowChartContainer parent = ControlTreeHelper.FindParentOfType<FlowChartContainer>(node.fatherNode);
            if (parent.IsWaiting)
            { return; }
            //进入选择模式
            parent.WaitClick(node);
        }
        #endregion

        /// <summary>
        /// 设置真正的Node
        /// </summary>
        public new void SetFather(NodeButton father)
        {
            this.fatherNode = father;
            yesNode.fatherNode = father;
            noNode.fatherNode = father;
        }
    }
}
