using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.Controls
{
    public class NodeBase: UserControl
    {
        //注册依赖属性，节点状态，有常规、选中、被搜索到
        public static readonly DependencyProperty NodeStateProperty =
            DependencyProperty.Register("NodeState",
                typeof(NodeState),
                typeof(NodeBase),
                new PropertyMetadata(NodeState.Normal));

        public NodeState NodeState
        {
            get { return (NodeState)GetValue(NodeStateProperty); }
            set { SetValue(NodeStateProperty, value); }
        }
    }
}