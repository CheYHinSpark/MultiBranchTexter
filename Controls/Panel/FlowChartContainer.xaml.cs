using MultiBranchTexter.Converters;
using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
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
using System.Collections.ObjectModel;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 流程图容器
    /// </summary>
    public partial class FlowChartContainer : UserControl
    {
        public static DependencyProperty IsModifiedProperty =
           DependencyProperty.Register("IsModified", //属性名称
               typeof(string), //属性类型
               typeof(FlowChartContainer), //该属性所有者，即将该属性注册到那个类上
               new PropertyMetadata("")//属性默认值
               );

        public string IsModified
        {
            get { return (string)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        //与拖拽、移动有关的变量
        private Point _clickPoint;
        //等待选择后继节点的node
        private NodeBase waitingNode;
        public bool IsWaiting { get { return waitingNode != null; } }
        //搜索相关的变量
        private int searchedIndex = -1;

        public FCCViewModel _viewModel;

        public FlowChartContainer()
        {
            InitializeComponent();
            _viewModel = DataContext as FCCViewModel;
        }

        #region 事件
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            searchBox.SetFlowChartContainer(this);
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
                if (Keyboard.Modifiers == ModifierKeys.None && e.LeftButton == MouseButtonState.Pressed)
                {
                    double x, y;
                    Point p = e.GetPosition((ScrollViewer)sender);

                    x = _clickPoint.X - p.X;
                    y = _clickPoint.Y - p.Y;

                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + x);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + y);

                    _clickPoint = e.GetPosition((ScrollViewer)sender);
                }
                else if (Keyboard.Modifiers == ModifierKeys.Alt && e.LeftButton == MouseButtonState.Pressed)
                {
                    double x, y;
                    Point p = e.GetPosition((ScrollViewer)sender);

                    x = p.X - _clickPoint.X;
                    y = p.Y - _clickPoint.Y;

                    for (int i =0;i<_viewModel.SelectedNodes.Count;i++)
                    {
                        _viewModel.SelectedNodes[i].Move(x / container.ScaleRatio, y / container.ScaleRatio);
                    }

                    _clickPoint = e.GetPosition((ScrollViewer)sender);

                    IsModified = "*";
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
                //更新点击点
                _clickPoint = e.GetPosition((ScrollViewer)sender);
                //右键点击，取消选择后继
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (IsWaiting)
                    {
                        PostNodeChoosed(null);
                    }
                    return;
                }
                //非右键点击
                if (Keyboard.Modifiers == ModifierKeys.Control)//同时按下Ctrl
                {
                    selectBorder.Visibility = Visibility.Visible;
                    selectBorder.StartPt = _clickPoint;
                    selectBorder.Width = 0;
                    selectBorder.Height = 0;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Alt)//同时按下了Alt，进入移动选中的节点模式
                {
                    this.Cursor = Cursors.Hand;
                }
                else//没有按下Ctrl
                {
                    //取消选择
                    _viewModel.ClearSelection();
                    stateHint.Text = "";
                    //准备拖拽
                    this.Cursor = Cursors.Hand;
                }
            }
        }

        private void scrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            //如果是完成多选
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
                _viewModel.ClearSelection();
                ObservableCollection<NodeButton> newSelect = new ObservableCollection<NodeButton>();
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
        #endregion


        #region 方法
        /// <summary>
        /// 输入文件路径加载之
        /// </summary>
        public void Load(string mbtxtPath)
        {
            container.Children.Clear();
            _viewModel.SelectedNodes.Clear();// <--必须
            MBFileReader reader = new MBFileReader(mbtxtPath);
            DrawFlowChart(reader.Read());
        }

        #region 流程图绘制方法
        /// <summary>
        ///  根据List<TextNode>建立树状图
        /// 不处理循坏连接的情况，有向无环图排序
        /// </summary>
        private void ReDrawFlowChart(List<TextNode> textNodes)
        {
            //nodeButtons总表
            List<NodeButton> nodeButtons = new List<NodeButton>();

            int num = textNodes.Count;
            if (num == 0)
            { 
                Debug.WriteLine("创建了空节点图");
                return;
            }
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
            Debug.WriteLine("节点图创建完成");
            IsModified = "*";
        }

        /// <summary>
        /// 根据List<TextNodeWithLeftTop>建立树状图，这个就简单许多
        /// </summary>
        private void DrawFlowChart(List<TextNodeWithLeftTop> textNodes)
        {
            Debug.WriteLine("开始绘制节点图");
            //nodeButtons总表
            List<NodeButton> nodeButtons = new List<NodeButton>();

            for (int i = 0; i < textNodes.Count; i++)
            {
                //初始化button
                nodeButtons.Add(new NodeButton(textNodes[i].Node));
            }
            Debug.WriteLine("节点创建完成");
            for (int i = 0; i < textNodes.Count; i++)
            {
                if (textNodes[i].Node.endCondition == null)
                {
                    int postIndex = textNodes[i].Node.GetPostNodeIndex(textNodes);
                    //为nodeButton添加post
                    if (postIndex >= 0)
                        NodeButton.Link(nodeButtons[i], nodeButtons[postIndex]);
                }
                else if (textNodes[i].Node.endCondition is YesNoCondition)
                {
                    YesNoCondition ync = textNodes[i].Node.endCondition as YesNoCondition;
                    for (int j = 0; j < textNodes.Count; j++)
                    {
                        if (ync.YesNode == textNodes[j].Node)
                        { NodeButton.Link(nodeButtons[i], nodeButtons[j], true); }
                        else if (ync.NoNode == textNodes[j].Node)
                        { NodeButton.Link(nodeButtons[i], nodeButtons[j], false); }
                    }
                }
                else if (textNodes[i].Node.endCondition is MultiAnswerCondition)
                {
                    MultiAnswerCondition mac = textNodes[i].Node.endCondition as MultiAnswerCondition;
                    for (int j = 0; j < mac.AnswerToNodes.Count; j++)
                    {
                        for (int k = 0; k < textNodes.Count; k++)
                        {
                            if (mac.AnswerToNodes[j].PostNode == textNodes[k].Node)
                            { NodeButton.Link(nodeButtons[i], nodeButtons[k], mac.AnswerToNodes[j].Answer); }
                        }
                    }
                }
            }
            Debug.WriteLine("节点链接完成");
            for (int i = 0; i < textNodes.Count; i++)
            {
                Canvas.SetLeft(nodeButtons[i], Math.Max(0, textNodes[i].Left));
                Canvas.SetTop(nodeButtons[i], Math.Max(0, textNodes[i].Top));
                container.Children.Add(nodeButtons[i]);
                nodeButtons[i].SetParent(container);
            }
            Debug.WriteLine("节点绘制完成");
            //添加线
            //连线现在放到别的地方，即nodebutton的loaded里面执行
            for (int i = 0; i < textNodes.Count; i++)
            {
                nodeButtons[i].ShowEndCondition();
                nodeButtons[i].AddPostLines(container);
            }
            Debug.WriteLine("节点图创建完成");
        }

        /// <summary>
        /// 重新绘制流程图
        /// </summary>
        /// <param name="newNode"></param>
        public void ReDrawFlowChart()
        {
            List<TextNode> textNodes = GetTextNodeList();
            container.Children.Clear();
            ReDrawFlowChart(textNodes);
        }

        public void AddNodeButton(NodeBase preNode,NodeButton postNode, double xPos, double yPos)
        {
            NodeButton nodeButton = new NodeButton(new TextNode(GetNewName(), ""));
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

            IsModified = "*";
        }
        #endregion

        #region 节点累计、获取List、获取新节点名字

        public List<TextNodeWithLeftTop> GetTextNodeWithLeftTopList()
        {
            List<TextNodeWithLeftTop> textNodes = new List<TextNodeWithLeftTop>();
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    textNodes.Add(new TextNodeWithLeftTop(
                        (control as NodeButton).textNode,
                        Canvas.GetLeft(control),
                        Canvas.GetTop(control)));//注意left和top都不受scale影响
                }
            }
            return textNodes;
        }

        /// <summary>
        /// 获得textNode列表
        /// </summary>
        public List<TextNode> GetTextNodeList()
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

        /// <summary>
        /// 获得一个新名字
        /// </summary>
        /// <returns></returns>
        public string GetNewName()
        {
            List<TextNode> tns = GetTextNodeList();
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

        public bool CheckRepeat(string newName)
        {
            List<TextNode> tns = GetTextNodeList();
            for (int j = 0; j < tns.Count; j++)
            {
                if (tns[j].Name == newName)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 节点搜索功能
        

        /// <summary>
        /// 根据信息搜索目标节点
        /// </summary>
        public void SearchNode(string info)
        {
            _viewModel.ClearSearch();
            if (info == "")
            { return; }
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    if ((control as NodeButton).BeSearch(info))
                    { _viewModel.SearchedNodes.Add((control as NodeButton)); }
                }
            }
            for (int i =0;i < _viewModel.SearchedNodes.Count;i++)
            {
                _viewModel.SearchedNodes[i].NodeState = NodeState.Searched;
            }
            //如果不为空，跳转到第一个查到的node
            if (_viewModel.SearchedNodes.Count > 0)
            {
                searchedIndex = 0;
                ScrollToNode(_viewModel.SearchedNodes[0]);
            }
        }

        public void ClearSearch()
        { _viewModel.ClearSearch(); }

        /// <summary>
        /// 根据节点找到
        /// </summary>
        public void SearchNode(TextNode node)
        {
            _viewModel.ClearSearch();
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    NodeButton nb = control as NodeButton;
                    if (nb.textNode == node)
                    {
                        _viewModel.SearchedNodes.Add(nb);
                        nb.NodeState = NodeState.Searched;
                        searchedIndex = 0;
                        ScrollToNode(nb);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 搜索到的下一个
        /// </summary>
        public void SearchNext(string info)
        {
            if (_viewModel.SearchedNodes.Count == 0)
            { 
                SearchNode(info);
                return;
            }
            searchedIndex++;
            if (searchedIndex >= _viewModel.SearchedNodes.Count)
            { searchedIndex = 0; }
            ScrollToNode(_viewModel.SearchedNodes[searchedIndex]);
        }

        /// <summary>
        /// 滚动到目标节点
        /// </summary>
        private void ScrollToNode(NodeButton node)
        {
            double x, y;
            x = Canvas.GetLeft(node) + node.ActualWidth / 2;
            y = Canvas.GetTop(node) + node.ActualHeight / 2;
            scrollViewer.ScrollToHorizontalOffset(x * container.ScaleRatio - scrollViewer.ActualWidth / 2);
            scrollViewer.ScrollToVerticalOffset(y * container.ScaleRatio - scrollViewer.ActualHeight / 2);
        }

        /// <summary>
        /// 滚动到目标节点
        /// </summary>
        public void ScrollToNode(TextNode node)
        {
            SearchNode(node);
        }
        #endregion

        #region 节点选择功能与选择新后继节点功能
        public void NewSelection(NodeButton node)
        {
            _viewModel.ClearSelection();
            stateHint.Text = "选中节点";
            _viewModel.SelectedNodes.Add(node);
            _viewModel.SelectedNodes[0].NodeState = NodeState.Selected;
        }
        public void NewSelection(ObservableCollection<NodeButton> nodes)
        {
            _viewModel.ClearSelection();
            for (int i =0; i < nodes.Count;i++)
            {
                nodes[i].NodeState = NodeState.Selected;
            }
            _viewModel.SelectedNodes = nodes;
            stateHint.Text = "选中节点";
        }

        public void AddSelection(NodeButton node)
        {
            node.NodeState = NodeState.Selected;
            _viewModel.SelectedNodes.Add(node);
        }

        public void AddSelection(List<NodeButton> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].NodeState = NodeState.Selected;
                _viewModel.SelectedNodes.Add(nodes[i]);
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
            //修改标签页
            (Application.Current.MainWindow as MainWindow).ReLoadTab(waitingNode.fatherNode.textNode);
            waitingNode = null;
        }
        #endregion

        #endregion
    }
}