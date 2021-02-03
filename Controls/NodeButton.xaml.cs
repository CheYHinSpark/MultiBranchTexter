using MultiBranchTexter.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
        public Border UpperBd;
        private TextBox titleBox;
        private Border endContainer;
        private NodeBase endNode;//这个只是挂在屁股下面的那个容器里的东西
        private AutoSizeCanvas parent;

        public TextNode textNode;
        public List<NodeButton> postNodes = new List<NodeButton>();
        public List<NodeButton> preNodes = new List<NodeButton>();

        public List<ConnectingLine> postLines = new List<ConnectingLine>();
        public List<ConnectingLine> preLines = new List<ConnectingLine>();

        // 移动相关
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
            UpperBd = GetTemplateChild("UpperBd") as Border;
            endContainer = GetTemplateChild("endContainer") as Border;
            endContainer.Child = endNode;
            titleBox = GetTemplateChild("titleBox") as TextBox;
            //显示标题
            titleBox.Text = textNode.Name;
            //设置显示顺序为2，以显示在connectingline上面
            Panel.SetZIndex(this, 2);
            DrawPostLines(ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this));
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
            titleBox.SelectionStart = 0;
            // TODO 检查重复
            textNode.Name = titleBox.Text;
            // TODO 通知窗体改变相应的标签页
        }
        #endregion

        #region 方法
        #region 静态方法
        public static void Link(NodeButton pre, NodeButton post)
        {
            //添加
            pre.postNodes.Add(post);
            post.preNodes.Add(pre);
            TextNode.Link(pre.textNode, post.textNode);
        }
        public static void Link(NodeButton pre, NodeButton post,bool yesno)
        {
            //添加
            pre.postNodes.Add(post);
            post.preNodes.Add(pre);
            TextNode.Link(pre.textNode, post.textNode, yesno);
        }
        public static void Link(NodeButton pre, NodeButton post,string answer)
        {
            //添加
            pre.postNodes.Add(post);
            post.preNodes.Add(pre);
            TextNode.Link(pre.textNode, post.textNode, answer);
        }
        public static void UnLink(NodeButton pre, NodeButton post)
        {
            //移除
            pre.postNodes.Remove(post);
            post.preNodes.Remove(pre);
            TextNode.UnLink(pre.textNode, post.textNode);
        }
        public static void UnLink(NodeButton pre, NodeButton post,bool yesno)
        {
            //移除
            pre.postNodes.Remove(post);
            post.preNodes.Remove(pre);
            TextNode.UnLink(pre.textNode, post.textNode, yesno);
        }
        public static void UnLink(NodeButton pre, NodeButton post,string answer)
        {
            //移除
            pre.postNodes.Remove(post);
            post.preNodes.Remove(pre);
            TextNode.UnLink(pre.textNode, post.textNode, answer);
        }

        /// <summary>
        /// 已经有相当信息的link，能根据信息自动选择连接方式
        /// </summary>
        public static void Link(NodeBase pre, NodeButton post)
        {
            if (pre.fatherNode.textNode.endCondition == null)
            { NodeButton.Link(pre.fatherNode, post); }
            else if (pre.fatherNode.textNode.endCondition is YesNoCondition)
            {
                if (pre.Name == "yesNode")
                { NodeButton.Link(pre.fatherNode, post, true); }
                else
                { NodeButton.Link(pre.fatherNode, post, false); }
            }
            else if (pre.fatherNode.textNode.endCondition is MultiAnswerCondition)
            {
                NodeButton.Link(pre.fatherNode, post, (pre as NodeEndMAAnswer).Answer);
            }
        }
        /// <summary>
        /// 已经有相当信息的unLink，能根据信息自动选择断开方式
        /// </summary>
        public static void UnLink(NodeBase pre, NodeButton post)
        {
            if (pre.fatherNode.textNode.endCondition == null)
            { NodeButton.UnLink(pre.fatherNode, post); }
            else if (pre.fatherNode.textNode.endCondition is YesNoCondition)
            {
                if (pre.Name == "yesNode")
                { NodeButton.UnLink(pre.fatherNode, post, true); }
                else
                { NodeButton.UnLink(pre.fatherNode, post, false); }
            }
            else if (pre.fatherNode.textNode.endCondition is MultiAnswerCondition)
            {
                NodeButton.UnLink(pre.fatherNode, post, (pre as NodeEndMAAnswer).Answer);
            }
        }
        #endregion

        #region 绘制与布局
        /// <summary>
        /// 设置父控件
        /// </summary>
        public void SetParent(AutoSizeCanvas canvas)
        {
            parent = canvas;
        }

        /// <summary>
        /// 显示后继条件框框
        /// </summary>
        public void ShowEndCondition()
        {
            fatherNode = this;
            if (textNode.endCondition == null)//表示是无选项的
            {
                //把可能存在的东西删掉
                if (endContainer != null)
                { endContainer.Child = null; }
            }
            else if (textNode.endCondition is YesNoCondition)
            {
                endNode = new NodeEndYesNo(textNode.endCondition as YesNoCondition);
                (endNode as NodeEndYesNo).SetFather(this);
            }
            else if (textNode.endCondition is MultiAnswerCondition)
            {
                endNode = new NodeEndMA(textNode.endCondition as MultiAnswerCondition);
                endNode.SetFather(this);
            }
        }


        /// <summary>
        /// 根据textNode的连接情况在自己和后续节点间生成连线
        /// </summary>
        /// <param name="container"></param>
        public void DrawPostLines(Panel container)
        {
            if (textNode.endCondition == null)//表示是无选项的
            {
                if (postNodes.Count == 0)
                { return; }
                ConnectingLine line = new ConnectingLine
                {
                    BeginNode = this,
                    EndNode = postNodes[0]
                };
                postLines.Add(line);
                postNodes[0].preLines.Add(line);
                container.Children.Add(line);
                //container.UpdateLayout();
                line.Drawing();
            }
            else if (textNode.endCondition is YesNoCondition)
            {
                NodeEndYesNo tempNode = endNode as NodeEndYesNo;
                ConnectingLine line1 = new ConnectingLine
                {
                    BeginNode = tempNode.yesNode,
                    EndNode = postNodes[0]
                };
                ConnectingLine line2 = new ConnectingLine
                {
                    BeginNode = tempNode.noNode,
                    EndNode = postNodes[1]
                };
                postNodes[0].preLines.Add(line1);
                postNodes[1].preLines.Add(line2);
                postLines.Add(line1);
                postLines.Add(line2);
                container.Children.Add(line1);
                container.Children.Add(line2);
                container.UpdateLayout();// <--没有会出错
                line1.Drawing();
                line2.Drawing();
            }
            else if (textNode.endCondition is MultiAnswerCondition)
            {

            }
        }
        #endregion

        #region 移动
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
        #endregion


        /// <summary>
       /// 根据输入的字符串，返回是否被查询到，当前只查询标题
       /// </summary>
        public bool BeSearch(string findStr)
        {
            return titleBox.Text.Contains(findStr);
        }


        #endregion
        public ConnectingLine GetPostLine(NodeButton post)
        {
            for (int i=0;i<postLines.Count;i++)
            {
                if (postLines[i].EndNode == post)
                {
                    return postLines[i];
                }
            }
            return null;
        }

        //上层bd被点击，这是在重新选择后继节点时可以被选中
        private void UpperBd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //通知流程图容器自己被选中
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).PostNodeChoosed(this);
        }
        //虽然0次引用，但是这是有用的，这是单一后继节点模式下重新选择后继节点功能
        private void NodeBase_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border)
            {
                //这样，就表示点击到了主题border上
                if ((e.OriginalSource as Border).Name == "bgBorder" && textNode.endCondition == null)
                {
                    FlowChartContainer parent = ControlTreeHelper.FindParentOfType<FlowChartContainer>(this);
                    if (parent.IsWaiting)
                    { return; }
                    //进入选择模式
                    parent.WaitClick(this);
                }
            }
        }
        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult warnResult = MessageBox.Show
                (
                ControlTreeHelper.FindParentOfType<MainWindow>(this),
                "确定要删除节点吗？\n这将同时断开节点的所有连接线，并且此操作不可撤销！",
                "警告",
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            FlowChartContainer fcc = ControlTreeHelper.FindParentOfType<FlowChartContainer>(this);
            List<ConnectingLine> readyToRemoveLines = new List<ConnectingLine>();
            while (preNodes.Count >0)
            {
                readyToRemoveLines.Clear();
                foreach (ConnectingLine line in preNodes[0].fatherNode.postLines)
                {
                    if (line.EndNode == this)
                    {
                        readyToRemoveLines.Add(line);
                        fcc.DeleteLine(line);
                    }
                }
                foreach (ConnectingLine line in readyToRemoveLines)
                {
                    preNodes[0].fatherNode.postLines.Remove(line);
                }
                NodeButton.UnLink(preNodes[0], this);
            }
            foreach (ConnectingLine line in postLines)
            {
                NodeButton.UnLink(line.BeginNode, line.EndNode);
                line.EndNode.preLines.Remove(line);
                fcc.DeleteLine(line);
            }
            fcc.container.Children.Remove(this);
            //TODO通知窗体把对应的标签删掉
        }

        private void ChangeEnd_Click(object sender, RoutedEventArgs e)
        {
            string header = (string)(sender as MenuItem).Header;
            if ((header == "单一后继" && textNode.endCondition == null)
                || (header == "判断后继" && textNode.endCondition is YesNoCondition)
                || (header == "多选后继" && textNode.endCondition is MultiAnswerCondition))
            { return; }
            MessageBoxResult warnResult = MessageBox.Show
                (
                ControlTreeHelper.FindParentOfType<MainWindow>(this),
                "确定要变更后继条件吗？\n这将同时断开节点的所有后继连接线以及与所有后继节点的连接。\n此操作不可撤销！",
                "警告",
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            FlowChartContainer fcc = ControlTreeHelper.FindParentOfType<FlowChartContainer>(this);
            foreach (ConnectingLine line in postLines)
            {
                NodeButton.UnLink(line.BeginNode, line.EndNode);
                line.EndNode.preLines.Remove(line);
                fcc.DeleteLine(line);
            }
            postLines.Clear();
            if (header == "单一后继")
            {
                textNode.endCondition = null;
                endContainer.Child = null;
            }
            else if (header == "判断后继")
            {
                textNode.endCondition = new YesNoCondition();
                NodeEndYesNo newNode = new NodeEndYesNo();
                newNode.SetFather(this);
                endContainer.Child = newNode;
            }
            else
            {

            }
            //TODO通知窗体把对应的标签改了
        }
    }
}