using MultiBranchTexter.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// NodeButton.xaml 的交互逻辑
    /// </summary>
    public partial class NodeButton : NodeBase
    {
        private TextBox titleBox;
        private AutoSizeCanvas parent;

        public TextNode textNode;
        public List<NodeButton> postNodes = new List<NodeButton>();

        public List<ConnectingLine> preLines = new List<ConnectingLine>();
        public List<ConnectingLine> postLines = new List<ConnectingLine>();
 
        private Point oldPoint = new Point();
        private bool isMoving = false;
        public bool IsMoving { get { return isMoving; } }
        public NodeButton()
        { }
        public NodeButton(TextNode newNode)
        {
            InitializeComponent();
            textNode = newNode;
        }


        #region 事件
        //加载完成
        private void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            titleBox = GetTemplateChild("titleBox") as TextBox;
            //显示标题
            titleBox.Text = textNode.Name;
            //设置显示顺序为2，以显示在connectingline上面
            Panel.SetZIndex(this, 2);
        }
        #region 移动事件
        private void nodeButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoving)
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    SwitchMoving();
                    return;
                }
                //移动自身位置
                Move(e.GetPosition(parent).X - oldPoint.X,
                    e.GetPosition(parent).Y - oldPoint.Y);
                oldPoint = e.GetPosition(parent);
            }
        }

        private void nodeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NodeState = NodeState.Selected;
            if (e.OriginalSource is Border)
            {
                SwitchMoving();
                //通知flowchart改变selectedNodes
                FlowChartContainer container = ControlTreeHelper.FindParentOfType<FlowChartContainer>(parent);
                container.NewSelection(this);
            }
            oldPoint = e.GetPosition(parent);
        }

        private void nodeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SwitchMoving();
        }

        private void nodeButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isMoving)
            {
                SwitchMoving();
            }
        }
        #endregion

        //点击editBtn……咦我想要什么功能来着
        private void editBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        //点击gotobtn，跳转到相应的标签页
        private void gotoBtn_Click(object sender, RoutedEventArgs e)
        {
            //通知窗体切换页面，打开相应的标签页
            ControlTreeHelper.FindParentOfType<MainWindow>(this).OpenMBTabItem(textNode);
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
        #region 静态方法
        public static void Link(NodeButton pre, NodeButton post)
        {
            //检查是否已经存在这个后节点

            //添加
            TextNode.Link(pre.textNode, post.textNode);
            pre.postNodes.Add(post);
        }
        public static void UnLink(NodeButton pre, NodeButton post)
        {
            //检查是否存在这个后节点

            //移除
            pre.postNodes.Remove(post);
            TextNode.UnLink(pre.textNode, post.textNode);
        }
        #endregion

        /// <summary>
        /// 根据textNode的连接情况在自己和后续节点间生成连线
        /// </summary>
        /// <param name="container"></param>
        public void DrawPostLines(Panel container)
        {
            foreach (NodeButton node in postNodes)
            {
                ConnectingLine line = new ConnectingLine
                {
                    BeginElement = this,
                    EndElement = node
                };
                postLines.Add(line);
                node.preLines.Add(line);
                container.Children.Add(line);
                container.UpdateLayout();// <--没有将无法显示
                line.Drawing();
            }
        }

        /// <summary>
        /// 根据一个后继节点自己和后续节点间生成连线
        /// </summary>
        /// <param name="container"></param>
        public void DrawPostLine(Panel container, NodeButton postNode)
        {
            if (!postNodes.Contains(postNode))
            { throw new System.Exception("没有目标后继节点"); }
            ConnectingLine line = new ConnectingLine
            {
                BeginElement = this,
                EndElement = postNode
            };
            postLines.Add(line);
            postNode.preLines.Add(line);
            container.Children.Add(line);
            container.UpdateLayout();// <--没有将无法显示
            line.Drawing();
        }


        /// <summary>
        /// 切换是否移动
        /// </summary>
        private void SwitchMoving()
        {
            // 如果正在移动，切换为不移动
            if (isMoving)
            {
                Panel.SetZIndex(this, 2);
                parent.IsResizing = false;
                isMoving = false;
                return;
            }
            Panel.SetZIndex(this, 3);
            parent.IsResizing = true;
            isMoving = true;
        }
        /// <summary>
        /// 移动方法
        /// </summary>
        public void Move(double xTrans, double yTrans)
        {
            xTrans += Canvas.GetLeft(this);
            yTrans += Canvas.GetTop(this);
            //自身位置
            xTrans = xTrans >= 0 ? xTrans : 0;
            yTrans = yTrans >= 0 ? yTrans : 0;
            Canvas.SetLeft(this, xTrans);
            Canvas.SetTop(this, yTrans);
            //调整线条
            foreach (ConnectingLine line in preLines)
            { line.Drawing(); }
            foreach (ConnectingLine line in postLines)
            { line.Drawing(); }
        }

        /// <summary>
        /// 设置父控件
        /// </summary>
        public void SetParent(AutoSizeCanvas canvas)
        {
            parent = canvas;
        }
        /// <summary>
       /// 根据输入的字符串，返回是否被查询到，当前只查询标题
       /// </summary>
        public bool BeSearch(string findStr)
        {
            return titleBox.Text.Contains(findStr);
        }
        #endregion

    }
}