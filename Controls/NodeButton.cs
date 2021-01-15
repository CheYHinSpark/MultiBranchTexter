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
        public List<NodeButton> postNodes = new List<NodeButton>();
        public NodeButton(TextNode newNode)
        {
            textNode = newNode;
            Content = textNode.Name;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Loaded += NodeButton_Loaded;
            Click += NodeButton_Click;
        }

        //点击btn
        private void NodeButton_Click(object sender, RoutedEventArgs e)
        {
            //通知窗体切换页面，打开相应的标签页
            (FindParentWindow(this)).OpenMBTabItem(textNode);
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
        public void DrawPostLines(Grid container)
        {
            foreach (NodeButton node in postNodes)
            {
                ConnectingLine line = new ConnectingLine();
                line.BeginElement = this;
                line.EndElement = node;
                container.Children.Add(line);
                container.UpdateLayout();// <--没有将无法显示
                line.StartDrawing();
            }
        }

        /// <summary>
        /// 递归找父级MainWindow
        /// </summary>
        /// <param name="reference">依赖对象</param>
        /// <returns>TabControl</returns>
        private MainWindow FindParentWindow(DependencyObject reference)
        {
            DependencyObject dObj = VisualTreeHelper.GetParent(reference);
            if (dObj == null)
                return null;
            if (dObj.GetType() == typeof(MainWindow))
                return dObj as MainWindow;
            else
                return FindParentWindow(dObj);
        }
    }
}
