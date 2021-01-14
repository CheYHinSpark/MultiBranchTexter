using MultiBranchTexter.Model;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiBranchTexter
{
    public class NodeButton : Button
    {

        public TextNode textNode;
        private List<NodeButton> postNodes = new List<NodeButton>();
        public NodeButton()
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Loaded += NodeButton_Loaded;
            //获取text的标题作为button的content

        }

        private void NodeButton_Loaded(object sender, RoutedEventArgs e)
        {
            //设置显示顺序为1，显示在connectingline上面
            Panel.SetZIndex(this, 1);
        }

        public void AddPostNode(NodeButton postButton)
        {
            //检查是否已经存在这个后节点

            //添加
            postNodes.Add(postButton);
        }

        //根据textNode的连接情况在自己和后续节点间连线
        public void DrawLines()
        {
            foreach (NodeButton node in postNodes)
            {
                ConnectingLine line = new ConnectingLine();
                Grid parent = VisualTreeHelper.GetParent(this) as Grid;
                parent.Children.Add(line);
                line.BeginElement = this;
                line.EndElement = node;
                line.StartDrawing();
            }
        }
    }
}
