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
        
        public NodeButton(TextNode newNode)
        {
            InitializeComponent();
            textNode = newNode;
            FatherNode = this;
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
            UpdateLayout();
            Debug.WriteLine("节点成功生成" + textNode.Name);
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
        private void GotoBtn_Click(object sender, RoutedEventArgs e)
        {
            //通知窗体切换页面，打开相应的标签页
            (Application.Current.MainWindow as MainWindow).OpenMBTabItem(textNode);
        }

        //双击标题，可以改标题
        private void TitleBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            titleBox.Focusable = true;
        }

        //标题失去焦点，恢复为不可聚焦并完成标题修改
        private void TitleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            titleBox.Focusable = false;
            titleBox.SelectionStart = 0;
            // 检查重复
            if (titleBox.Text != textNode.Name)
            {
                if (ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).CheckRepeat(titleBox.Text))
                {
                    titleBox.Text = textNode.Name;
                    MessageBox.Show("节点名称重复，已还原");
                }
                else
                {
                    //没有重复，完成修改
                    textNode.Name = titleBox.Text;
                    //通知窗体改变相应的标签页
                    (Application.Current.MainWindow as MainWindow).ReLoadTab(textNode);
                }
            }
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
                if ((e.OriginalSource as Border).Name == "bgBorder" && textNode.endCondition.EndType == EndType.Single)
                {
                    FlowChartContainer parent = ControlTreeHelper.FindParentOfType<FlowChartContainer>(this);
                    if (parent.IsWaiting)
                    { return; }
                    //进入选择模式
                    parent.WaitClick(this);
                }
            }
        }

        #region 右键菜单功能
        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult warnResult = MessageBox.Show
                (
                Application.Current.MainWindow,
                "确定要删除节点吗？\n这将同时断开节点的所有连接线，并且此操作不可撤销！",
                "警告",
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            Delete();
        }

        private void ChangeEnd_Click(object sender, RoutedEventArgs e)
        {
            string header = (string)(sender as MenuItem).Header;
            if ((header == "单一后继" && textNode.endCondition.EndType == EndType.Single)
                || (header == "判断后继" && textNode.endCondition.EndType == EndType.YesNo)
                || (header == "多选后继" && textNode.endCondition.EndType == EndType.MultiAnswers))
            { return; }
            MessageBoxResult warnResult = MessageBox.Show
                (
                Application.Current.MainWindow,
                "确定要变更后继条件吗？\n这将同时断开节点的所有后继连接线以及与所有后继节点的连接。\n此操作不可撤销！",
                "警告",
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            UnLinkAllPost();//清除后继
            if (header == "单一后继")
            {
                textNode.endCondition = new EndCondition();
                endContainer.Child = null;
            }
            else if (header == "判断后继")
            {
                textNode.endCondition = new EndCondition(EndType.YesNo);
                NodeEndYesNo newNode = new NodeEndYesNo();
                newNode.SetFather(this);
                endContainer.Child = newNode;
            }
            else
            {
                textNode.endCondition = new EndCondition(EndType.MultiAnswers);
                NodeEndMA newNode = new NodeEndMA();
                newNode.SetFather(this);
                endContainer.Child = newNode;
            }
            //通知窗体把对应的标签改了
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
            (Application.Current.MainWindow as MainWindow).ReLoadTab(textNode);
        }
        #endregion

        #endregion

        #region 方法

        #region 静态方法
        //public static void Link(NodeButton pre, NodeButton post)
        //{
        //    //添加
        //    pre.postNodes.Add(post);
        //    post.preNodes.Add(pre);
        //    TextNode.Link(pre.textNode, post.textNode, "");
        //}

        /// <summary>
        /// 连接，注意不能在对键值的遍历中搞这个
        /// </summary>
        public static void Link(NodeButton pre, NodeButton post,string answer)
        {
            //添加
            pre.postNodes.Add(post);
            post.preNodes.Add(pre);
            TextNode.Link(pre.textNode, post.textNode, answer);
        }
        //public static void UnLink(NodeButton pre, NodeButton post)
        //{
        //    //移除
        //    pre.postNodes.Remove(post);
        //    post.preNodes.Remove(pre);
        //    TextNode.UnLink(pre.textNode, post.textNode);
        //}

        /// <summary>
        /// 断开，注意不能在对键值的遍历中搞这个
        /// </summary>
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
            switch (pre.FatherTextNode.endCondition.EndType)
            {
                case EndType.MultiAnswers:
                    Link(pre.FatherNode, post, (pre as NodeEndMAAnswer).Answer);
                    break;
                case EndType.YesNo:
                    if (pre.Name == "yesNode")
                    { Link(pre.FatherNode, post, "yes"); }
                    if (pre.Name == "noNode")
                    { Link(pre.FatherNode, post, "no"); }
                    break;
                case EndType.Single:
                    Link(pre.FatherNode, post, ""); break;
            }
        }
        /// <summary>
        /// 已经有相当信息的unLink，能根据信息自动选择断开方式
        /// </summary>
        public static void UnLink(NodeBase pre, NodeButton post)
        {
            switch (pre.FatherTextNode.endCondition.EndType)
            {
                case EndType.MultiAnswers:
                    UnLink(pre.FatherNode, post, (pre as NodeEndMAAnswer).Answer);
                    break;
                case EndType.YesNo:
                    if (pre.Name == "yesNode")
                    { UnLink(pre.FatherNode, post, "yes"); }
                    if (pre.Name == "noNode")
                    { UnLink(pre.FatherNode, post, "no"); }
                    break;
                case EndType.Single:
                    UnLink(pre.FatherNode, post, ""); break;
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
            FatherNode = this;

            switch (textNode.endCondition.EndType)
            {
                case EndType.Single:
                    //把可能存在的东西删掉
                    if (endContainer != null)
                    { endContainer.Child = null; }
                    break;
                case EndType.YesNo:
                    endNode = new NodeEndYesNo(textNode.endCondition);
                    (endNode as NodeEndYesNo).SetFather(this);
                    break;
                case EndType.MultiAnswers:
                    endNode = new NodeEndMA(textNode.endCondition);
                    (endNode as NodeEndMA).SetFather(this);
                    break;
            }
        }


        /// <summary>
        /// 根据textNode的连接情况在自己和后续节点间生成连线
        /// </summary>
        public void DrawPostLines(Panel container)
        {
            switch (textNode.endCondition.EndType)
            {
                case EndType.Single:
                    if (postNodes.Count == 0)//可能没有
                    { return; }
                    ConnectingLine line = new ConnectingLine(this, postNodes[0]);
                    container.Children.Add(line);
                    break;
                case EndType.YesNo:

                    foreach (NodeButton nodeButton in postNodes)
                    {
                        if (textNode.endCondition.Answers["yes"] == nodeButton.textNode.Name)
                        {
                            container.Children.Add(
                                new ConnectingLine((endNode as NodeEndYesNo).yesNode, nodeButton)
                                );
                        }
                        if (textNode.endCondition.Answers["no"] == nodeButton.textNode.Name)
                        {
                            container.Children.Add(
                                new ConnectingLine((endNode as NodeEndYesNo).noNode, nodeButton)
                                );
                        }
                    }
                    break;

                case EndType.MultiAnswers:
                    NodeEndMA tempNode = endNode as NodeEndMA;
                    for (int i = 0; i < postNodes.Count; i++)
                    {
                        foreach (UserControl control in tempNode.answerContainer.Children)
                        {
                            if (textNode.endCondition.Answers[(control as NodeEndMAAnswer).Answer] 
                                == postNodes[i].textNode.Name)
                            {
                                container.Children.Add(new ConnectingLine(control as NodeBase, postNodes[i]));
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 刷新前驱线
        /// </summary>
        public void UpdatePreLines()
        {
            for (int i = 0; i < preLines.Count; i++)
            {
                preLines[i].Update();
            }
        }

        /// <summary>
        /// 刷新后继线
        /// </summary>
        public void UpdatePostLines()
        {
            for (int i = 0; i < postLines.Count; i++)
            {
                postLines[i].Update();
            }
        }

        /// <summary>
        /// 断开自身与所有后继节点，删除所有后继连线
        /// </summary>
        private void UnLinkAllPost()
        {
            while (postLines.Count > 0)
            {
                NodeButton.UnLink(postLines[0].BeginNode, postLines[0].EndNode);
                postLines[0].Delete();
            }
        }

        /// <summary>
        /// 获得中心坐标，不受放缩影响
        /// </summary>
        public Vector GetCenter()
        {
            return new Vector(Canvas.GetLeft(this) + 50, Canvas.GetTop(this) + 25);
        }
        #endregion

        #region 移动

        /// <summary>
        /// 切换是否移动
        /// </summary>
        public void SwitchMoving()
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
            { line.Update(); }
            foreach (ConnectingLine line in postLines)
            { line.Update(); }
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
        }
        #endregion

        /// <summary>
        /// 删除该节点，这将同时去掉所有连线
        /// </summary>
        public void Delete()
        {
            //清除所有前驱
            while (preLines.Count > 0)
            {
                NodeButton.UnLink(preLines[0].BeginNode, preLines[0].EndNode);
                preLines[0].Delete();
            }
            UnLinkAllPost();//清除所有后继
            //通知窗体把对应的标签删掉
            (Application.Current.MainWindow as MainWindow).DeleteTab(textNode);
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
            //删掉自己
            ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this).Children.Remove(this);
        }

        /// <summary>
        /// 根据输入的字符串，返回是否被查询到，当前只查询标题
        /// </summary>
        public bool BeSearch(string findStr)
        {
            return titleBox.Text.Contains(findStr);
        }

        /// <summary>
        /// 输入一条前驱连接线，返回其应有的偏移量
        /// </summary>
        public Vector GetPreLineEndOffset(ConnectingLine line)
        {
            Vector point = new Vector(50, 25);
            if (preLines.Count == 1)
            { return point; }
            for (int i = 0;i < preLines.Count; i++)
            {
                if (preLines[i] == line)
                {
                    point.X += -30 + 60.0 * i / (preLines.Count - 1);
                    return point;
                }
            }
            return point;
        }
        #endregion
    }
}