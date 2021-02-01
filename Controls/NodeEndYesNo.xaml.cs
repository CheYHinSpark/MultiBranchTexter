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
        public YesNoCondition endCondition = new YesNoCondition();
        public NodeEndYesNo()
        {
            InitializeComponent();
        }

        public NodeEndYesNo(YesNoCondition yesNoCond)
        {
            InitializeComponent();
            endCondition = yesNoCond;
        }

        //加载完成
        private void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            //显示标题
            titleBox.Text = endCondition.Question;
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
            // TODO 检查并完成标题修改
        }

        public void SetFather(NodeButton father)
        {
            yesNode.fatherNode = father;
            noNode.fatherNode = father;
        }
    }
}
