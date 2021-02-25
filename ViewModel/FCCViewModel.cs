using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class FCCViewModel : ViewModelBase
    {
        #region 字段
        //搜索相关的变量
        private int searchedIndex = -1;

        private Visibility _searchBoxVisibility;
        public Visibility SearchBoxVisibility
        {
            get { return _searchBoxVisibility; }
            set
            { _searchBoxVisibility = value; RaisePropertyChanged("SearchBoxVisibility"); }
        }
        private ObservableCollection<NodeButton> _selectedNodes;
        public ObservableCollection<NodeButton> SelectedNodes
        {
            get { return _selectedNodes; }
            set
            { _selectedNodes = value; RaisePropertyChanged("SelectedNodes"); }
        }
        private ObservableCollection<NodeButton> _searchedNodes;
        public ObservableCollection<NodeButton> SearchedNodes
        {
            get { return _searchedNodes; }
            set
            { _searchedNodes = value; RaisePropertyChanged("SearchedNodes"); }
        }

        #region 虽然这样做不太符合mvvm的理念，但我不想再搞了
        private ScrollViewer _scrollViewer;
        public ScrollViewer ScrollViewer
        { set { _scrollViewer = value; } }
        private AutoSizeCanvas _container;
        public AutoSizeCanvas Container
        { set { _container = value; } }
        #endregion
        #endregion

        #region 命令
        public ICommand NewNodeCommand => new RelayCommand((t) =>
        {
            Point point = Mouse.GetPosition(_container);
            NodeButton newNode = new NodeButton(new TextNode(GetNewName()));
            newNode.SetParent(_container);
            _container.Children.Add(newNode);
            Canvas.SetLeft(newNode, Math.Max(0, point.X - 50));
            Canvas.SetTop(newNode, Math.Max(0, point.Y - 25));
            Debug.WriteLine("新建节点成功");
            ViewModelFactory.Main.IsModified = "*";
        });

        public ICommand UniteXCommand => new RelayCommand((sender) =>
        {
            string mode = (string)sender;
            double nX = Canvas.GetLeft(SelectedNodes[0]);
            if (mode == "avg")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                { nX += Canvas.GetLeft(SelectedNodes[i]); }
                nX /= SelectedNodes.Count;
            }
            else if (mode == "min")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetLeft(SelectedNodes[i]);
                    if (nX > temp)
                    { nX = temp; }
                }
            }
            else
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetLeft(SelectedNodes[i]);
                    if (nX < temp)
                    { nX = temp; }
                }
            }
            //统一
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Canvas.SetLeft(SelectedNodes[i], nX);
                SelectedNodes[i].UpdatePostLines();
                SelectedNodes[i].UpdatePreLines();
            }
            ViewModelFactory.Main.IsModified = "*";
        });

        public ICommand UniteYCommand => new RelayCommand((sender) =>
        {
            string mode = (string)sender;
            double nY = Canvas.GetTop(SelectedNodes[0]);
            if (mode == "avg")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                { nY += Canvas.GetTop(SelectedNodes[i]); }
                nY /= SelectedNodes.Count;
            }
            else if (mode == "min")
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetTop(SelectedNodes[i]);
                    if (nY > temp)
                    { nY = temp; }
                }
            }
            else
            {
                for (int i = 1; i < SelectedNodes.Count; i++)
                {
                    double temp = Canvas.GetTop(SelectedNodes[i]);
                    if (nY < temp)
                    { nY = temp; }
                }
            }
            //统一
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Canvas.SetTop(SelectedNodes[i], nY);
                SelectedNodes[i].UpdatePostLines();
                SelectedNodes[i].UpdatePreLines();
            }
            ViewModelFactory.Main.IsModified = "*";
        });

        public ICommand DeleteCommand => new RelayCommand((sender) =>
        {
            if (SelectedNodes.Count == 0)
            { return; }
            Debug.WriteLine("准备删除节点");
            int n = SelectedNodes.Count;
            MessageBoxResult warnResult = MessageBox.Show
                (
                Application.Current.MainWindow,
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
            SelectedNodes.Clear();
            Debug.WriteLine("删除了" + n.ToString() + "个节点");
            ViewModelFactory.Main.IsModified = "*";
        });

        public ICommand StartSearchCommand => new RelayCommand((t) =>
        { SearchBoxVisibility = Visibility.Visible; });
        #endregion

        public FCCViewModel()
        {
            SearchBoxVisibility = Visibility.Hidden;
            SelectedNodes = new ObservableCollection<NodeButton>();
            SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;
            SearchedNodes = new ObservableCollection<NodeButton>();
            SearchedNodes.CollectionChanged += SearchedNodes_CollectionChanged;
        }

        #region 事件
        private void SearchedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        { RaisePropertyChanged("SearchedNodes"); }

        private void SelectedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        { RaisePropertyChanged("SelectedNodes"); }
        #endregion

        #region 方法

        /// <summary> 输入文件路径加载之 </summary>
        public void Load(string mbtxtPath)
        {
            _container.Children.Clear();
            ViewModelFactory.Main.IsModified = "";
            SelectedNodes.Clear();// <--必须
            var nodes = MetadataFile.ReadNodes(mbtxtPath);
            DrawFlowChart(nodes);
        }

        #region 流程图绘制方法
        /// <summary>
        /// 根据List<TextNode>重新建立树状图
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
                { mat[i, j] = false; }
                //一开始都未完成
                hasDone[i] = false;
                //初始化组数
                groupIndexOfBtns[i] = 0;
            }
            for (int i = 0; i < textNodes.Count; i++)
            {
                EndCondition ec = textNodes[i].endCondition;

                Dictionary<string, int> waitToLink = new Dictionary<string, int>();

                foreach (string answer in ec.Answers.Keys)
                {
                    if (ec.Answers[answer] == "")
                    { continue; }//直接跳过空的

                    bool found = false;
                    for (int k = 0; k < num; k++)
                    {
                        if (ec.Answers[answer] == textNodes[k].Name)
                        {
                            found = true;
                            //准备为nodeButton添加post
                            waitToLink.Add(answer, k);

                            break;
                        }
                    }
                    if (!found)
                    { waitToLink.Add(answer, -1); }
                }

                foreach (string answer in waitToLink.Keys)
                {
                    if (waitToLink[answer] == -1)
                    { ec.Answers[answer] = ""; }
                    else
                    {
                        mat[i, waitToLink[answer]] = true;
                        NodeButton.Link(nodeButtons[i], nodeButtons[waitToLink[answer]], answer);
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
                    { mat[hasDoneIndex[i], j] = false; }
                }
            }
            //根据分组开始放置NodeButton
            for (int i = 0; i < groupedNodes.Count; i++)
            {
                for (int j = 0; j < groupedNodes[i].Count; j++)
                {
                    Canvas.SetLeft(groupedNodes[i][j], 60 + 160 * j);
                    Canvas.SetTop(groupedNodes[i][j], 80 + 240 * i);
                    _container.Children.Add(groupedNodes[i][j]);
                    groupedNodes[i][j].SetParent(_container);
                }
            }
            //添加线
            //连线现在放到别的地方，即nodebutton的loaded里面执行
            Debug.WriteLine("节点图创建完成");
            ViewModelFactory.Main.IsModified = "*";
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
                EndCondition ec = textNodes[i].Node.endCondition;

                Dictionary<string, int> waitToLink = new Dictionary<string, int>();

                foreach (string answer in ec.Answers.Keys)
                {
                    if (ec.Answers[answer] == "")
                    { continue; }//直接跳过空的

                    bool found = false;
                    for (int k = 0; k < textNodes.Count; k++)
                    {
                        if (ec.Answers[answer] == textNodes[k].Node.Name)
                        {
                            found = true;
                            //准备为nodeButton添加post
                            waitToLink.Add(answer, k);

                            break;
                        }
                    }
                    if (!found)
                    { waitToLink.Add(answer, -1); }
                }

                foreach (string answer in waitToLink.Keys)
                {
                    if (waitToLink[answer] == -1)
                    { ec.Answers[answer] = ""; }
                    else
                    { NodeButton.Link(nodeButtons[i], nodeButtons[waitToLink[answer]], answer); }
                }
            }
            Debug.WriteLine("节点链接完成");
            for (int i = 0; i < textNodes.Count; i++)
            {
                Canvas.SetLeft(nodeButtons[i], Math.Max(0, textNodes[i].Left));
                Canvas.SetTop(nodeButtons[i], Math.Max(0, textNodes[i].Top));
                _container.Children.Add(nodeButtons[i]);
                nodeButtons[i].SetParent(_container);
            }
            Debug.WriteLine("节点绘制完成");
            //添加线
            //连线现在放到别的地方，即nodebutton的loaded里面执行
            Debug.WriteLine("节点图创建完成");
        }

        /// <summary> 重新绘制流程图 </summary>
        public void ReDrawFlowChart()
        {
            List<TextNode> textNodes = GetTextNodeList();
            _container.Children.Clear();
            ReDrawFlowChart(textNodes);
        }
        #endregion

        #region 选择与搜索节点相关
        public void ClearSelection()
        {
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                if (SearchedNodes.Contains(SelectedNodes[i]))
                { SelectedNodes[i].NodeState = NodeState.Searched; }
                else
                { SelectedNodes[i].NodeState = NodeState.Normal; }
            }
            SelectedNodes.Clear();
        }

        public void ClearSearch()
        {
            for (int i = 0; i < SearchedNodes.Count; i++)
            {
                if (SelectedNodes.Contains(SearchedNodes[i]))
                { SearchedNodes[i].NodeState = NodeState.Selected; }
                else
                { SearchedNodes[i].NodeState = NodeState.Normal; }
            }
            SearchedNodes.Clear();
        }

        public void NewSelection(NodeButton node)
        {
            ClearSelection();
            SelectedNodes.Add(node);
            SelectedNodes[0].NodeState = NodeState.Selected;
        }
        public void NewSelection(ObservableCollection<NodeButton> nodes)
        {
            ClearSelection();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].NodeState = NodeState.Selected;
            }
            SelectedNodes = nodes;
            //stateHint.Text = "选中节点";
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
        #endregion

        #region 节点累计、获取List、新节点名字与重复检查
        /// <summary> 获得带有左上角位置信息的节点表 </summary>
        public List<TextNodeWithLeftTop> GetTextNodeWithLeftTopList()
        {
            Debug.WriteLine("开始尝试取得节点与坐标信息");
            List<TextNodeWithLeftTop> textNodes = new List<TextNodeWithLeftTop>();
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    textNodes.Add(new TextNodeWithLeftTop(
                        (control as NodeButton).textNode,
                        Canvas.GetLeft(control),
                        Canvas.GetTop(control)));//注意left和top都不受scale影响
                }
            }
            Debug.WriteLine("成功取得节点与坐标信息");
            return textNodes;
        }

        /// <summary> 获得textNode列表  </summary>
        public List<TextNode> GetTextNodeList()
        {
            List<TextNode> textNodes = new List<TextNode>();
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    textNodes.Add((control as NodeButton).textNode);
                }
            }
            return textNodes;
        }

        /// <summary>获得一个新名字</summary>
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
                    if (tns[j].Name == newName + i.ToString())
                    {
                        repeated = true;
                        i++;
                        break;
                    }
                }
            }
            return newName + i.ToString();
        }

        public bool CheckRepeat(string newName)
        {
            List<TextNode> tns = GetTextNodeList();
            for (int j = 0; j < tns.Count; j++)
            {
                if (tns[j].Name == newName)
                { return true; }
            }
            return false;
        }

        public TextNode GetNodeByName(string name)
        {
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    if ((control as NodeButton).textNode.Name == name)
                    { return (control as NodeButton).textNode; }
                }
            }
            return null;
        }
        #endregion

        #region 添加节点
        public void AddNodeButton(NodeBase preNode, NodeButton postNode, double xPos, double yPos)
        {
            NodeButton newNodeButton = new NodeButton(new TextNode(GetNewName()));
            newNodeButton.SetParent(_container);
            newNodeButton.FatherNode = newNodeButton;
            //连接三个点
            NodeButton.Link(preNode, newNodeButton);
            NodeButton.Link(newNodeButton, postNode);
            _container.Children.Add(newNodeButton);
            Canvas.SetLeft(newNodeButton, xPos);
            Canvas.SetTop(newNodeButton, yPos);
            _container.UpdateLayout();
            newNodeButton.Move(-newNodeButton.ActualWidth / 2, -newNodeButton.ActualHeight / 2);
            //画新线
            _container.Children.Add(new ConnectingLine(preNode, newNodeButton));
            //newNodeButton到post不用画，因为创建newNodebutton时会自动画，否则会出现两条

            ViewModelFactory.Main.IsModified = "*";
        }
        #endregion

        #region 节点搜索定位功能
        /// <summary> 根据信息搜索目标节点 </summary>
        public void SearchNode(string info)
        {
            ClearSearch();
            if (info == "")
            { return; }
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    if ((control as NodeButton).BeSearch(info))
                    { SearchedNodes.Add((control as NodeButton)); }
                }
            }
            for (int i = 0; i < SearchedNodes.Count; i++)
            {
                SearchedNodes[i].NodeState = NodeState.Searched;
            }
            //如果不为空，跳转到第一个查到的node
            if (SearchedNodes.Count > 0)
            {
                searchedIndex = 0;
                ScrollToNode(SearchedNodes[0]);
            }
        }

        /// <summary> 搜索到的下一个 </summary>
        public void SearchNext(string info)
        {
            if (SearchedNodes.Count == 0)
            {
                SearchNode(info);
                return;
            }
            searchedIndex++;
            if (searchedIndex >= SearchedNodes.Count)
            { searchedIndex = 0; }
            ScrollToNode(SearchedNodes[searchedIndex]);
        }

        /// <summary> 滚动到目标节点 </summary>
        private void ScrollToNode(NodeButton node)
        {
            _scrollViewer.UpdateLayout();// <--需要
            double x, y;
            x = Canvas.GetLeft(node) + node.ActualWidth / 2;
            y = Canvas.GetTop(node) + node.ActualHeight / 2;
            _scrollViewer.ScrollToHorizontalOffset(x * _container.ScaleRatio - _scrollViewer.ActualWidth / 2);
            _scrollViewer.ScrollToVerticalOffset(y * _container.ScaleRatio - _scrollViewer.ActualHeight / 2);
        }

        /// <summary> 定位到目标节点 </summary>
        public void LocateToNode(TextNode node)
        {
            ClearSearch();
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    NodeButton nb = control as NodeButton;
                    if (nb.textNode == node)
                    {
                        SearchedNodes.Add(nb);
                        nb.NodeState = NodeState.Searched;
                        searchedIndex = 0;
                        ScrollToNode(nb);
                        return;
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
