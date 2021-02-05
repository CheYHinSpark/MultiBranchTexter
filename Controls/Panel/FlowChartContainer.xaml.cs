using MultiBranchTexter.Converters;
using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Data;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 流程图容器
    /// </summary>
    public partial class FlowChartContainer : FlowChartBase
    {
        //与拖拽、移动有关的变量
        private Point _clickPoint;
        private bool isDragScroll = false;
        //等待选择后继节点的node
        private NodeBase waitingNode;
        public bool IsWaiting { get { return waitingNode != null; } }
        //搜索相关的变量
        public List<NodeButton> searchedNodes = new List<NodeButton>();
        private int searchedIndex = -1;
        

        public FlowChartContainer()
        {
            InitializeComponent();
        }

        #region 事件
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            searchBox.SetFlowChartContainer(this);
            //测试读取Test文件夹下的mbtxt文件
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "Test\\test.mbtxt";
            MBFileReader reader = new MBFileReader(path);
            //reader.Read();
            DrawFlowChart(reader.Read());

            //绑定数据
            Binding binding1 = new Binding
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("SelectedNodes"),
                Converter = new ListToBoolConverter(),
                ConverterParameter = 1//为两个统一坐标的Para
            };
            Binding binding2 = new Binding
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("SelectedNodes"),
                Converter = new ListToBoolConverter(),
            };
            BindingOperations.SetBinding(unitXMenuItem, IsEnabledProperty, binding1);
            BindingOperations.SetBinding(unitYMenuItem, IsEnabledProperty, binding1);
            BindingOperations.SetBinding(deleteItem, IsEnabledProperty, binding2);
        }

        //滚轮事件
        private void scrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // 按住Ctrl，开始放大缩小
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                container.ScaleRatio += 0.1 * Math.Sign(e.Delta);
                container.ScaleRatio = container.ScaleRatio < 0.1 ? 0.1 : container.ScaleRatio;
            }
        }

        //鼠标移动
        private void scrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Grid)
            {
                if (isDragScroll)
                {
                    double x, y;
                    Point p = e.GetPosition((ScrollViewer)sender);

                    x = _clickPoint.X - p.X;
                    y = _clickPoint.Y - p.Y;

                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + x);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + y);

                    _clickPoint = e.GetPosition((ScrollViewer)sender);
                }
                else { this.Cursor = Cursors.Arrow; }
            }
            if (selectBorder.Visibility == Visibility.Visible)
            {
                Point p = e.GetPosition((ScrollViewer)sender);
                selectBorder.SetEndPt(p.X, p.Y);
            }
        }

        //点击scrollViewer
        private void scrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //表示点击到了空白部分
            if (e.OriginalSource is Grid)
            {
                //右键点击，取消选择后继
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (IsWaiting)
                    {
                        PostNodeChoosed(null);
                    }
                    //更新点击点
                    _clickPoint = e.GetPosition((ScrollViewer)sender);
                    return;
                }
                //非右键点击
                if (Keyboard.Modifiers == ModifierKeys.Control)//同时按下Ctrl
                {
                    selectBorder.Visibility = Visibility.Visible;
                    selectBorder.StartPt = e.GetPosition((ScrollViewer)sender);
                    selectBorder.Width = 0;
                    selectBorder.Height = 0;
                }
                else//没有按下Ctrl
                {
                    //取消选择
                    ClearSelection();
                    stateHint.Text = "";
                    //准备拖拽
                    isDragScroll = true;
                    this.Cursor = Cursors.Hand;
                    _clickPoint = e.GetPosition((ScrollViewer)sender);
                }
            }
        }

        private void scrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragScroll = false;
            this.Cursor = Cursors.Arrow;
            //如果是多选
            if (selectBorder.Visibility == Visibility.Visible)
            {
                //需要得到selectBd关于canvas的坐标
                //获得了canvas相对于自身的坐标，这个point是经过放缩的
                GeneralTransform gt = container.TransformToAncestor(this);
                Point point = gt.Transform(new Point(0, 0));
                point.X *= -1;//需要负号
                point.Y *= -1;
                Point lt = point + selectBorder.GetLeftTop();
                Point rb = point + selectBorder.GetRightBottom();
                List<NodeButton> newSelect = new List<NodeButton>();
                foreach (UserControl control in container.Children)
                {
                    if (control is NodeButton)
                    {
                        Vector ct = (control as NodeButton).GetCenter() * container.ScaleRatio;
                        //如果在selecBox内部
                        if (lt.X < ct.X && lt.Y < ct.Y
                            && rb.X > ct.X && rb.Y > ct.Y)
                        {
                            newSelect.Add(control as NodeButton);
                        }
                    }
                }
                selectBorder.Visibility = Visibility.Hidden;
                NewSelection(newSelect);
            }
        }


        #region 右键菜单
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            //需要得到selectBd关于canvas的坐标
            //获得了canvas相对于scrollviewer的坐标，这个point是经过放缩的
            GeneralTransform gt = container.TransformToAncestor(this);
            Point point = gt.Transform(new Point(0, 0));
            point.X *= -1;//需要负号
            point.Y *= -1;
            point.X += _clickPoint.X;
            point.Y += _clickPoint.Y;
            NodeButton newNode = new NodeButton(new TextNode(GetNewName(), ""));
            newNode.SetParent(container);
            container.Children.Add(newNode);
            Canvas.SetLeft(newNode, Math.Max(0, point.X / container.ScaleRatio - 50));
            Canvas.SetTop(newNode, Math.Max(0, point.Y / container.ScaleRatio - 25));
        }

        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            int n = SelectedNodes.Count;
            MessageBoxResult warnResult = MessageBox.Show
                (
                ControlTreeHelper.FindParentOfType<MainWindow>(this),
                "你即将删除" + n.ToString()
                + "个节点！\n这将同时断开这些节点的所有连接线，并且此操作不可撤销！",
                "警告",
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            while (SelectedNodes.Count > 0)
            {
                SelectedNodes[0].Delete();
                SelectedNodes.RemoveAt(0);
            }
            SelectedNodes = new List<NodeButton>();
        }

        private void XItem_Click(object sender, RoutedEventArgs e)
        {
            string mode = (sender as MenuItem).Name;
            double nX = Canvas.GetLeft(SelectedNodes[0]);
            if (mode == "xavg")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    nX += Canvas.GetLeft(SelectedNodes[i]);
                }
                nX /= SelectedNodes.Count;
            }
            else if (mode == "xmin")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetLeft(SelectedNodes[i]);
                    if (nX > temp)
                    {
                        nX = temp;
                    }
                }
            }
            else
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetLeft(SelectedNodes[i]);
                    if (nX < temp)
                    {
                        nX = temp;
                    }
                }
            }
            //统一
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Canvas.SetLeft(SelectedNodes[i], nX);
                SelectedNodes[i].UpdatePostLines();
                SelectedNodes[i].UpdatePreLines();
            }
        }
        private void YItem_Click(object sender, RoutedEventArgs e)
        {
            string mode = (sender as MenuItem).Name;
            double nY = Canvas.GetTop(SelectedNodes[0]);
            if (mode == "yavg")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    nY += Canvas.GetTop(SelectedNodes[i]);
                }
                nY /= SelectedNodes.Count;
            }
            else if (mode == "ymin")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetTop(SelectedNodes[i]);
                    if (nY > temp)
                    {
                        nY = temp;
                    }
                }
            }
            else
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetTop(SelectedNodes[i]);
                    if (nY < temp)
                    {
                        nY = temp;
                    }
                }
            }
            //统一
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Canvas.SetTop(SelectedNodes[i], nY);
                SelectedNodes[i].UpdatePostLines();
                SelectedNodes[i].UpdatePreLines();
            }
        }
        #endregion

        #endregion


        #region 方法

        #region 流程图绘制方法
        //根据List<TextNode>建立树状图
        //不处理循坏连接的情况
        //有向无环图排序
        private void DrawFlowChart(List<TextNode> textNodes)
        {
            //nodeButtons总表
            List<NodeButton> nodeButtons = new List<NodeButton>();

            int num = textNodes.Count;
            //node所在组数，实际是画图后的行数
            int[] groupIndexOfBtns = new int[num];
            //二维List，对NodeButton分组
            List<List<NodeButton>> groupedNodes = new List<List<NodeButton>>();
            //是否完成数组
            bool[] hasDone = new bool[num];
            //已完成的node指标
            List<int> hasDoneIndex = new List<int>();
            //初始化矩阵
            bool[,] mat = new bool[num, num];
            //初始化
            for (int i = 0; i < num; i++)
            {
                //初始化button
                nodeButtons.Add(new NodeButton(textNodes[i]));
                //初始化矩阵
                for (int j = 0; j < num; j++)
                {
                    mat[i, j] = false;
                }
                //一开始都未完成
                hasDone[i] = false;
                //初始化组数
                groupIndexOfBtns[i] = 0;
            }
            for (int i = 0; i < textNodes.Count; i++)
            {
                if (textNodes[i].endCondition == null)
                {
                    List<int> postIndex = textNodes[i].GetPostNodeIndex(textNodes);
                    //为矩阵赋值，如果i有j作为后继，则ij位置为真
                    for (int j = 0; j < postIndex.Count; j++)
                    {
                        mat[i, postIndex[j]] = true;
                        //为nodeButton添加post
                        NodeButton.Link(nodeButtons[i], nodeButtons[postIndex[j]]);
                    }
                }
                else if (textNodes[i].endCondition is YesNoCondition)
                {
                    YesNoCondition ync = textNodes[i].endCondition as YesNoCondition;
                    for (int j = 0; j < textNodes.Count; j++)
                    {
                        if (ync.YesNode == textNodes[j])
                        {
                            mat[i, j] = true;
                            NodeButton.Link(nodeButtons[i],nodeButtons[j], true);
                        }
                        else if (ync.NoNode == textNodes[j])
                        {
                            mat[i, j] = true;
                            NodeButton.Link(nodeButtons[i], nodeButtons[j], false);
                        }
                    }
                }
                else if (textNodes[i].endCondition is MultiAnswerCondition)
                {
                    MultiAnswerCondition mac = textNodes[i].endCondition as MultiAnswerCondition;
                    for (int j =0;j < mac.AnswerToNodes.Count;j++)
                    {
                        for (int k = 0; k < textNodes.Count; k++)
                        {
                            if (mac.AnswerToNodes[j].PostNode == textNodes[k])
                            {
                                mat[i, k] = true;
                                NodeButton.Link(nodeButtons[i], nodeButtons[k], mac.AnswerToNodes[j].Answer);
                            }
                        }
                    }
                }
            }
            while (hasDoneIndex.Count < num)
            {
                //搜寻尚未完成的node中无前驱者
                List<NodeButton> noPreNodes = new List<NodeButton>();
                for (int j = 0; j < num; j++)
                {
                    //已经做了，跳过
                    if (hasDoneIndex.Contains(j))
                    { continue; }
                    bool shouldAdd = true;
                    for (int i = 0; i < num; i++)
                    {
                        //找到一个前驱，跳过
                        if (mat[i, j])
                        {
                            shouldAdd = false;
                            break;
                        }
                    }
                    if (!shouldAdd)
                    { continue; }
                    //未找到前驱
                    noPreNodes.Add(nodeButtons[j]);
                    hasDoneIndex.Add(j);
                }
                //添加
                groupedNodes.Add(noPreNodes);
                //处理身后事
                for (int i = 0; i < hasDoneIndex.Count; i++)
                {
                    for (int j = 0; j < num; j++)
                    {
                        mat[hasDoneIndex[i], j] = false;
                    }
                }
            }
            //根据分组开始放置NodeButton
            for (int i = 0; i < groupedNodes.Count; i++)
            {
                for (int j = 0; j < groupedNodes[i].Count; j++)
                {
                    Canvas.SetLeft(groupedNodes[i][j], 60 + 160 * j);
                    Canvas.SetTop(groupedNodes[i][j], 80 + 240 * i);
                    container.Children.Add(groupedNodes[i][j]);
                    groupedNodes[i][j].SetParent(container);
                }
            }
            //添加线
            //连线现在放到别的地方，即nodebutton的loaded里面执行
            for (int i = 0; i < textNodes.Count; i++)
            {
                nodeButtons[i].ShowEndCondition();
                nodeButtons[i].AddPostLines(container);
            }
            Debug.WriteLine("节点创建完成");
        }

        
        /// <summary>
        /// 重新绘制流程图
        /// </summary>
        /// <param name="newNode"></param>
        public void ReDrawFlowChart()
        {
            List<TextNode> textNodes = GetTextNodes();
            container.Children.Clear();
            DrawFlowChart(textNodes);
        }

        public void AddNodeButton(TextNode newNode,NodeBase preNode,NodeButton postNode, double xPos, double yPos)
        {
            NodeButton nodeButton = new NodeButton(newNode);
            nodeButton.SetParent(container);
            nodeButton.fatherNode = nodeButton;
            //连接三个点
            NodeButton.Link(preNode, nodeButton);
            NodeButton.Link(nodeButton, postNode);
            container.Children.Add(nodeButton);
            Canvas.SetLeft(nodeButton, xPos);
            Canvas.SetTop(nodeButton, yPos);
            container.UpdateLayout();
            nodeButton.Move(-nodeButton.ActualWidth / 2, -nodeButton.ActualHeight / 2);
            //画两条线
            preNode.DrawPostLine(container, nodeButton);
            nodeButton.DrawPostLine(container, postNode);
        }
        #endregion

        #region 节点累计、获取List、获取新节点名字
        /// <summary>
        /// 获得textNode列表
        /// </summary>
        public List<TextNode> GetTextNodes()
        {
            List<TextNode> textNodes = new List<TextNode>();
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    textNodes.Add((control as NodeButton).textNode);
                }
            }
            return textNodes;
        }

        public string GetNewName()
        {
            List<TextNode> tns = GetTextNodes();
            //创造一个不重名的
            string newName = "new-node-";
            int i = 1;
            bool repeated = true;
            while (repeated)
            {
                repeated = false;
                for (int j = 0; j < tns.Count; j++)
                {
                    if (tns[j].Name == newName + i.ToString() + ".txt")
                    {
                        repeated = true;
                        i++;
                        break;
                    }
                }
            }
            return newName + i.ToString() + ".txt";
        }
        #endregion

        #region 节点搜索功能
        public void ClearSearch()
        {
            for (int i =0; i < searchedNodes.Count;i++)
            {
                if (SelectedNodes.Contains(searchedNodes[i]))
                {
                    searchedNodes[i].NodeState = NodeState.Selected;
                }
                else
                {
                    searchedNodes[i].NodeState = NodeState.Normal;
                }
            }
            searchedNodes.Clear();
        }

        /// <summary>
        /// 根据信息搜索目标节点
        /// </summary>
        /// <param name="info"></param>
        public void SearchNode(string info)
        {
            ClearSearch();
            if (info == "")
            { return; }
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    if ((control as NodeButton).BeSearch(info))
                    { searchedNodes.Add((control as NodeButton)); }
                }
            }
            for (int i =0;i<searchedNodes.Count;i++)
            {
                searchedNodes[i].NodeState = NodeState.Searched;
            }
            //如果不为空，跳转到第一个查到的node
            if (searchedNodes.Count > 0)
            {
                searchedIndex = 0;
                ScrollToNode(searchedNodes[0]);
            }
        }

        /// <summary>
        /// 搜索到的下一个
        /// </summary>
        public void SearchNext(string info)
        {
            if (searchedNodes.Count == 0)
            { 
                SearchNode(info);
                return;
            }
            searchedIndex++;
            if (searchedIndex >= searchedNodes.Count)
            { searchedIndex = 0; }
            ScrollToNode(searchedNodes[searchedIndex]);
        }

        /// <summary>
        /// 滚动到目标节点
        /// </summary>
        /// <param name="node"></param>
        private void ScrollToNode(NodeButton node)
        {
            double x, y;
            x = Canvas.GetLeft(node) + node.ActualWidth / 2;
            y = Canvas.GetTop(node) + node.ActualHeight / 2;
            scrollViewer.ScrollToHorizontalOffset(x * container.ScaleRatio - scrollViewer.ActualWidth / 2);
            scrollViewer.ScrollToVerticalOffset(y * container.ScaleRatio - scrollViewer.ActualHeight / 2);
        }
        #endregion

        #region 节点选择功能与选择新后继节点功能
        public void NewSelection(NodeButton node)
        {
            ClearSelection();
            stateHint.Text = "选中节点";
            SelectedNodes = new List<NodeButton> { node };//这里不能用Add，否则无法通知到右键菜单
            SelectedNodes[0].NodeState = NodeState.Selected;
        }
        public void NewSelection(List<NodeButton> nodes)
        {
            ClearSelection();
            for (int i =0; i < nodes.Count;i++)
            {
                nodes[i].NodeState = NodeState.Selected;
            }
            SelectedNodes = nodes;
            stateHint.Text = "选中节点";
        }
        public void ClearSelection()
        {
            for (int i =0;i<SelectedNodes.Count;i++)
            {
                if (searchedNodes.Contains(SelectedNodes[i]))
                {
                    SelectedNodes[i].NodeState = NodeState.Searched;
                }
                else
                {
                    SelectedNodes[i].NodeState = NodeState.Normal;
                }
            }
            SelectedNodes = new List<NodeButton>();//只能这样不能Clear，否则无法通知到右键菜单
        }

        public void AddSelection(NodeButton node)
        {
            node.NodeState = NodeState.Selected;
            SelectedNodes.Add(node);
        }

        public void AddSelection(List<NodeButton> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].NodeState = NodeState.Selected;
                SelectedNodes.Add(nodes[i]);
            }
        }

        /// <summary>
        /// 进入等待点击以选择一个新的后继节点的状态
        /// </summary>
        public void WaitClick(NodeBase waiter)
        {
            waitingNode = waiter;
            stateHint.Text = "选择一个后继节点";
            //开启等待点击
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    //开启nodebutton的上层border，等待点击其中一个
                    (control as NodeButton).UpperBd.Visibility = Visibility.Visible;
                }
            }
            waiter.fatherNode.UpperBd.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 新的后继节点选择完成了，传入null表示取消选择
        /// </summary>
        public void PostNodeChoosed(NodeButton post)
        {
            stateHint.Text = "";
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    (control as NodeButton).UpperBd.Visibility = Visibility.Hidden;
                }
            }
            if (post == null)//没有选择
            {
                waitingNode = null;
                return;
            }
            //首先断开waitNode原有的连线
            ConnectingLine cline = null;//找到原本的连线
            foreach (ConnectingLine line in waitingNode.fatherNode.postLines)
            {
                if (line.BeginNode == waitingNode)
                { cline = line; }
            }
            if (cline != null)//不一定有这条原本连线
            {
                NodeButton.UnLink(waitingNode, cline.EndNode);
                waitingNode.fatherNode.postLines.Remove(cline);
                cline.EndNode.preLines.Remove(cline);
                container.Children.Remove(cline);
            }
            //在waitNode和Post之间连线
            NodeButton.Link(waitingNode, post);
            ConnectingLine cl = new ConnectingLine(waitingNode, post);
            container.Children.Add(cl);
            waitingNode = null;
        }
        #endregion

        #endregion
    }
}