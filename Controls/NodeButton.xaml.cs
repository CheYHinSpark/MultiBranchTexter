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

namespace MultiBranchTexter
{
    /// <summary>
    /// NodeButton.xaml 的交互逻辑
    /// </summary>
    public partial class NodeButton : UserControl
    {

        public TextNode textNode;
        public List<NodeButton> postNodes = new List<NodeButton>();
        public NodeButton()
        { }
        public NodeButton(TextNode newNode)
        {
            InitializeComponent();
            textNode = newNode;
            //显示标题
            titleBox.Text = textNode.Name;
            //设置显示顺序为1，以显示在connectingline上面
            Panel.SetZIndex(this, 1);
        }


        #region 事件
        //点击editBtn……咦我想要什么功能来着
        private void editBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        //点击gotobtn，跳转到相应的标签页
        private void gotoBtn_Click(object sender, RoutedEventArgs e)
        {
            //通知窗体切换页面，打开相应的标签页
            (FindParentWindow(this)).OpenMBTabItem(textNode);
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
        #endregion

        #region 方法
        //添加后续节点的NodeButton
        public void AddPostNode(NodeButton postButton)
        {
            //检查是否已经存在这个后节点

            //添加
            postNodes.Add(postButton);
        }

        //根据textNode的连接情况在自己和后续节点间生成连线
        public void DrawPostLines(Grid container)
        {
            foreach (NodeButton node in postNodes)
            {
                ConnectingLine line = new ConnectingLine
                {
                    BeginElement = this,
                    EndElement = node
                };
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
        #endregion
    }
}


