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

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// FlowChartContainer.xaml 的交互逻辑
    /// </summary>
    public partial class FlowChartContainer : UserControl
    {
        //与拖拽、移动有关的变量
        private Point _clickPoint;
        private bool isDragScroll = false;
        //选择相关的变量
        public List<NodeButton> selectedNodes = new List<NodeButton>();

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
                // TODO: 没有解决框选框负宽度问题
                Point p = e.GetPosition((ScrollViewer)sender);
                //selectBorder.Width = p.X - _clickPoint.X;
                //selectBorder.Height = p.Y - _clickPoint.Y;
            }
        }

        //点击scrollViewer
        private void scrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //表示点击到了空白部分
            if (e.OriginalSource is Grid)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    selectBorder.Visibility = Visibility.Visible;
                    _clickPoint = e.GetPosition((ScrollViewer)sender);
                    selectBorder.Margin = new Thickness(_clickPoint.X, _clickPoint.Y, 0, 0);
                    selectBorder.Width = 0;
                    selectBorder.Height = 0;
                }
                else
                {
                    //取消选择
                    ClearSelection();
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
            selectBorder.Visibility = Visibility.Hidden;
        }
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
                List<int> postIndex = textNodes[i].GetPostNodeIndex(textNodes);
                //为矩阵赋值，如果i有j作为后继，则ij位置为真
                for (int j = 0; j < postIndex.Count; j++)
                {
                    mat[i, postIndex[j]] = true;
                    //为nodeButton添加post
                    NodeButton.Link(nodeButtons[i], nodeButtons[postIndex[j]]);
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
                    Canvas.SetLeft(groupedNodes[i][j], 40 + 120 * j);
                    Canvas.SetTop(groupedNodes[i][j], 60 + 160 * i);
                    container.Children.Add(groupedNodes[i][j]);
                    groupedNodes[i][j].SetParent(container);
                }
            }
            //连线
            for (int i = 0; i < textNodes.Count; i++)
            {
                nodeButtons[i].DrawPostLines(container);
            }
        }

        //删除连线
        public void DeleteLine(ConnectingLine line)
        {
            container.Children.Remove(line);
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

        public void AddNodeButton(TextNode newNode,NodeButton preNode,NodeButton postNode, double xPos, double yPos)
        {
            NodeButton nodeButton = new NodeButton(newNode);
            nodeButton.SetParent(container);
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
            nodeButton.DrawPostLine(container,postNode);
        }
        #endregion

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

        #region 节点搜索功能
        public void ClearSearch()
        {
            for (int i =0;i<searchedNodes.Count;i++)
            {
                if (selectedNodes.Contains(searchedNodes[i]))
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

        #region 节点选择功能
        public void NewSelection(NodeButton node)
        {
            ClearSelection();
            selectedNodes.Add(node);
            selectedNodes[0].NodeState = NodeState.Selected;
        }
        public void NewSelection(List<NodeButton> nodes)
        {
            ClearSelection();
            for (int i =0; i<nodes.Count;i++)
            {
                nodes[i].NodeState = NodeState.Selected;
            }
            selectedNodes = nodes;
        }
        public void ClearSelection()
        {
            for (int i =0;i<selectedNodes.Count;i++)
            {
                if (searchedNodes.Contains(selectedNodes[i]))
                {
                    selectedNodes[i].NodeState = NodeState.Searched;
                }
                else
                {
                    selectedNodes[i].NodeState = NodeState.Normal;
                }
            }
            selectedNodes.Clear();
        }

        public void AddSelection(NodeButton node)
        {
            node.NodeState = NodeState.Selected;
            selectedNodes.Add(node);
        }
        public void AddSelection(List<NodeButton> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].NodeState = NodeState.Selected;
                selectedNodes.Add(nodes[i]);
            }
        }
        #endregion

        #endregion


    }
}