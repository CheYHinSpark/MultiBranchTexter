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
using MultiBranchTexter.Model;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// NodeEndMA.xaml 的交互逻辑
    /// </summary>
    public partial class NodeEndMA : NodeBase
    {
        public NodeEndMA()
        {
            InitializeComponent();
        }
        public NodeEndMA(MultiAnswerCondition mac)
        {
            InitializeComponent();
            titleBox.Text = mac.Question;
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
            // TODO 还要通知窗口改变相应的标签页
        }
        #endregion

        /// <summary>
        /// 设置问题文本
        /// </summary>
        /// <param name="q"></param>
        public void SetQuestion(string q)
        {
            titleBox.Text = q;
        }
    }
}
