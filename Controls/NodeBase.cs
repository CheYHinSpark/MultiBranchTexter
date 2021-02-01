using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 节点基类
    /// </summary>
    public class NodeBase : UserControl
    {
        public NodeButton fatherNode;

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

        /// <summary>
        /// 根据一个后继节点自己和后续节点间生成连线
        /// </summary>
        /// <param name="container"></param>
        public void DrawPostLine(Panel container, NodeButton postNode)
        {
            ConnectingLine line = new ConnectingLine
            {
                BeginNode = this,
                EndNode = postNode
            };
            fatherNode.postLines.Add(line);
            postNode.preLines.Add(line);
            container.Children.Add(line);
            container.UpdateLayout();// <--没有将无法显示
            line.Drawing();
        }
    }
}