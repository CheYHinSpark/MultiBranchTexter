using MultiBranchTexter.View;
using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Threading.Tasks;
using MultiBranchTexter.Resources;
using System.Windows.Threading;

namespace MultiBranchTexter.ViewModel
{
    public class FCCViewModel : ViewModelBase
    {
        #region 字段
        public HashSet<TextNode> Nodes { get; set; }

        #region 节点搜索、选择、节点累计
        //搜索相关的变量
        private int _searchedIndex;
        public int SearchedIndex 
        {
            get { return _searchedIndex; }
            set 
            {
                try
                { SearchedNodes[_searchedIndex].NodeState = NodeState.Searched; }
                catch { }

                _searchedIndex = value;

                try
                { SearchedNodes[_searchedIndex].NodeState = NodeState.TopSearched; }
                catch { }
                RaisePropertyChanged("SearchedIndex");
            }
        }

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

        private int _nodeCount;
        public int NodeCount
        {
            get { return _nodeCount; }
            set { _nodeCount = value;RaisePropertyChanged("NodeCount"); }
        }
        #endregion

        #region 没画完的Node
        private int _undrawedNode;
        public int UndrawedNode
        {
            get { return _undrawedNode; }
            set
            {
                _undrawedNode = value;
                if (value == 0)// 节点全部生成了，再画线
                { DrawLines(); }
                RaisePropertyChanged("UndrawedNode");
            }
        }
        #endregion

        #region 字数词数统计
        private int _charCount;
        public int CharCount
        {
            get { return _charCount; }
            set
            { _charCount = value; RaisePropertyChanged("CharCount"); }
        }

        private int _wordCount;
        public int WordCount
        {
            get { return _wordCount; }
            set
            { _wordCount = value; RaisePropertyChanged("WordCount"); }
        }
        #endregion

        #region 等待选择后继节点
        private NodeBase waitingNode;

        public bool IsWaiting { get { return waitingNode != null; } }
        #endregion

        #region 容器，虽然这样做不太符合mvvm的理念，但我不想再搞了
        private ScrollViewer _scrollViewer;
        public ScrollViewer ScrollViewer
        { 
            get { return _scrollViewer; }
            set { _scrollViewer = value; }
        }
        private AutoSizeCanvas _container;
        public AutoSizeCanvas Container
        {
            get { return _container; }
            set { _container = value; }
        }
        #endregion

        #endregion

        #region 命令
        public ICommand NewNodeCommand => new RelayCommand((_) =>
        {
            Point point = Mouse.GetPosition(_container);
            NodeButton newNode = new NodeButton(new TextNode(GetNewName()));
            _container.Children.Add(newNode);
            Canvas.SetLeft(newNode, Math.Max(0, point.X - 50));
            Canvas.SetTop(newNode, Math.Max(0, point.Y - 25));
            Debug.WriteLine("新建节点成功");
            ViewModelFactory.Main.IsModified = true;
            NodeCount = GetNodeCount();
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
                SelectedNodes[i].UpdateLines();
            }
            ViewModelFactory.Main.IsModified = true;
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
                SelectedNodes[i].UpdateLines();
            }
            ViewModelFactory.Main.IsModified = true;
        });

        public ICommand DeleteCommand => new RelayCommand((_) =>
        {
            if (SelectedNodes.Count == 0)
            { return; }
            Debug.WriteLine("准备删除节点");
            int n = SelectedNodes.Count;
            MessageBoxResult warnResult = MessageBox.Show
                (
                Application.Current.MainWindow,
                LanguageManager.Instance["Msg_MultiDeletePre"] + n.ToString()
                + LanguageManager.Instance["Msg_MultiDeletePost"],
                LanguageManager.Instance["Win_Warn"],
                MessageBoxButton.YesNo
                );
            if (warnResult == MessageBoxResult.No)
            { return; }
            while (SelectedNodes.Count > 0)
            {
                if (SearchedNodes.Contains(SelectedNodes[0]))
                {
                    SearchedNodes.Remove(SelectedNodes[0]);
                }
                SelectedNodes[0].Delete();
                SelectedNodes.RemoveAt(0);
            }
            SelectedNodes.Clear();
            Debug.WriteLine("删除了" + n.ToString() + "个节点");
            ViewModelFactory.Main.IsModified = true;
            NodeCount = GetNodeCount();
        });

        public ICommand StartSearchCommand => new RelayCommand((_) =>
        { SearchBoxVisibility = Visibility.Visible; });

        /// <summary> 重新绘制流程图 </summary>
        public ICommand ReArrangeCommand => new RelayCommand((_) =>
        {
            MessageBoxResult warnResult = MessageBox.Show
                            (
                            Application.Current.MainWindow,
                            LanguageManager.Instance["Msg_Rearrange"],
                            LanguageManager.Instance["Win_Warn"],
                            MessageBoxButton.YesNo
                            );
            if (warnResult == MessageBoxResult.No)
            { return; }
            ReDrawFlowChart();
        });

        /// <summary> 全选 </summary>
        public ICommand SelectAllCommand => new RelayCommand((_) =>
        { SelectAll(); });
        #endregion

        public FCCViewModel()
        {
            SearchBoxVisibility = Visibility.Hidden;
            SelectedNodes = new ObservableCollection<NodeButton>();
            SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;
            SearchedNodes = new ObservableCollection<NodeButton>();
            SearchedNodes.CollectionChanged += SearchedNodes_CollectionChanged;
            NodeCount = 0;
            _searchedIndex = -1;
            Nodes = new HashSet<TextNode>();
        }

        #region 事件
        private void SearchedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SearchedNodes.Count > 0)
            { 
                ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_Searched"] 
                    + SearchedNodes.Count.ToString() 
                    + LanguageManager.Instance["Hint_Nodes"]); 
            }
            else
            {
                ViewModelFactory.Main.QuickEndHint();
            }
            RaisePropertyChanged("SearchedNodes");
        }

        private void SelectedNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedNodes.Count > 0)
            {
                ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_Selected"]
                      + SelectedNodes.Count.ToString()
                      + LanguageManager.Instance["Hint_Nodes"]);
            }
            else
            {
                ViewModelFactory.Main.QuickEndHint();
            }
            RaisePropertyChanged("SelectedNodes");
        }
        #endregion

        #region 方法

        /// <summary> 输入文件路径加载之 </summary>
        public async void Load(string mbtxtPath)
        {
            await _container.Dispatcher.BeginInvoke(new Action(
                delegate { _container.Children.Clear(); }), DispatcherPriority.ApplicationIdle);
            Nodes.Clear();
            SelectedNodes.Clear();// <--必须
            try
            {
                var nodes = MetadataFile.ReadTextNodes(mbtxtPath);
                if (nodes.Count > 0)
                {
                    await _container.Dispatcher.BeginInvoke(new Action(
                        delegate { DrawFlowChart(nodes); }), DispatcherPriority.Background);
                }
            }
            catch
            {
                try
                {
                    var nodes = MetadataFile.ReadText(mbtxtPath);
                    if (!ViewModelFactory.Main.FileName.EndsWith(".mbjson"))
                    { ViewModelFactory.Main.FileName += ".mbjson"; }
                    ViewModelFactory.Main.IsModified = true;
                    await _container.Dispatcher.BeginInvoke(new Action(
                        delegate { DrawFlowChart(nodes); }), DispatcherPriority.Background);
                }
                catch
                {
                    ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_OpenFailed"]);
#if DEBUG
                    throw new FormatException("全部木大");
#endif
                }
            }
            Debug.WriteLine("Load结束");
        }

        #region 字词统计
        public async void CountCharWord(bool totalReCount)
        {
            await Task.Delay(10);// <--不然有许多bug
            await Task.Run(new Action(
                delegate
                {
                    int c = 0, w = 0;
                    foreach (TextNode node in Nodes)
                    {
                        foreach (TextFragment tfp in node.Fragments)
                        {
                            tfp.ShouldRecount |= totalReCount;
                            var ccw = tfp.CountCharWord();
                            c += ccw.Item1;
                            w += ccw.Item2;
                        }
                    }
                    CharCount = c;
                    WordCount = w;
                }));
        }
        #endregion

        #region 流程图绘制方法
        /// <summary>
        /// 重新建立树状图
        /// 不能处理循坏连接的情况
        /// 有向无环图排序
        /// </summary>
        private void ReDrawFlowChart()
        {
            waitingNode = null;

            //nodeButtons总表
            List<NodeButton> nodeButtons = new List<NodeButton>();
            List<TextNode> _Nodes = new List<TextNode>();

            foreach (UIElement element in Container.Children)
            {
                if (element is NodeButton)
                {
                    _Nodes.Add((element as NodeButton).TextNode);
                    nodeButtons.Add((element as NodeButton));
                }
            }

            int num = Nodes.Count;
            if (num == 0)
            {
                Debug.WriteLine("创建了空节点图");
                return;
            }
            //node所在组数，实际是画图后的行数
            int[] groupIndexOfBtns = new int[num];
            //二维List，对NodeButton分组
            List<List<NodeButton>> groupedNodes = new List<List<NodeButton>>();
            //已完成的node指标
            List<int> hasDoneIndex = new List<int>();
            //初始化矩阵
            bool[,] mat = new bool[num, num];

            //初始化
            for (int i = 0; i < num; i++)
            {
                //初始化矩阵
                for (int j = 0; j < num; j++)
                { mat[i, j] = false; }
                //初始化组数
                groupIndexOfBtns[i] = 0;
            }

            for (int i = 0; i < num; i++)
            {
                EndCondition ec = _Nodes[i].EndCondition;

                for (int j = 0; j < ec.Answers.Count; j++)
                {
                    if (ec.Answers[j].Item2 == "")
                    { continue; }//直接跳过空的

                    bool found = false;
                    for (int k = 0; k < num; k++)
                    {
                        if (ec.Answers[j].Item2 == _Nodes[k].Name)
                        {
                            found = true;
                            //准备为nodeButton添加post
                            mat[i, k] = true;
                            break;
                        }
                    }
                    if (!found)
                    { ec.Answers[j] = (ec.Answers[j].Item1, "", ec.Answers[j].Item3); }
                }
            }

            int oldIndexCount;
            while (hasDoneIndex.Count < num)
            {
                // 防止出现循环
                oldIndexCount = hasDoneIndex.Count;

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

                if (oldIndexCount == hasDoneIndex.Count)
                {
                    //出现循环，这个时候只能主动删掉一个节点的全部前驱
                    for (int j = 0; j < num; j++)
                    {
                        if (!hasDoneIndex.Contains(j))
                        {
                            for (int i = 0; i < num; i++)
                            { mat[i, j] = false; }
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < groupedNodes.Count; i++)
            {
                for (int j = 0; j < groupedNodes[i].Count; j++)
                {
                    Canvas.SetLeft(groupedNodes[i][j], 60 + 160 * j);
                    Canvas.SetTop(groupedNodes[i][j], 80 + 240 * i);
                }
            }

            for (int i = 0; i < num; i++)
            { nodeButtons[i].UpdateLines(); }

            Debug.WriteLine("节点图重绘完成");
            ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_Rearrange"]);
            ViewModelFactory.Main.IsModified = true;
        }

        /// <summary>
        /// 重新建立树状图
        /// 不能处理循坏连接的情况
        /// 有向无环图排序
        /// </summary>
        [Obsolete]
        private async void ReDrawFlowChart(List<TextNode> textNodes)
        {
            waitingNode = null;
            //TODO Nodes = textNodes.;
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
            await _container.Dispatcher.BeginInvoke(new Action(
                delegate
                {
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
                }));
            await _container.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    for (int i = 0; i < textNodes.Count; i++)
                    {
                        EndCondition ec = textNodes[i].EndCondition;

                        Dictionary<string, int> waitToLink = new Dictionary<string, int>();

                        for (int j = 0; j < ec.Answers.Count; j++)
                        {
                            if (ec.Answers[j].Item2 == "")
                            { continue; }//直接跳过空的

                            bool found = false;
                            for (int k = 0; k < num; k++)
                            {
                                if (ec.Answers[j].Item2 == textNodes[k].Name)
                                {
                                    found = true;
                                    //准备为nodeButton添加post
                                    NodeButton.Link(nodeButtons[i], nodeButtons[k], ec.Answers[j].Item1);
                                    mat[i, k] = true;
                                    break;
                                }
                            }
                            if (!found)
                            { ec.Answers[j] = (ec.Answers[j].Item1, "", ec.Answers[j].Item3); }
                        }
                    }
                }));
            await _container.Dispatcher.BeginInvoke(new Action(
                delegate
                {
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
                }));
            //根据分组开始放置NodeButton
            await _container.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    for (int i = 0; i < groupedNodes.Count; i++)
                    {
                        for (int j = 0; j < groupedNodes[i].Count; j++)
                        {
                            Canvas.SetLeft(groupedNodes[i][j], 60 + 160 * j);
                            Canvas.SetTop(groupedNodes[i][j], 80 + 240 * i);
                            _container.Children.Add(groupedNodes[i][j]);
                        }
                    }
                }));
            //添加线
            //连线现在放到别的地方，即nodebutton的loaded里面执行
            Debug.WriteLine("节点图重绘完成");
            ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_Rearrange"]);
            NodeCount = num;
        }

        /// <summary>
        /// 根据List<TextNodeWithLeftTop>建立树状图，这个就简单许多
        /// </summary> 
        private void DrawFlowChart(List<TextNodeWithLeftTop> textNodes)
        {
            waitingNode = null;
            Debug.WriteLine("开始绘制节点图");
            int num = textNodes.Count;
            UndrawedNode = num;
            //nodeButtons总表
            List<NodeButton> nodeButtons = new List<NodeButton>();

            for (int i = 0; i < num; i++)
            {
                Nodes.Add(textNodes[i].Node);
                //初始化button
                nodeButtons.Add(new NodeButton(textNodes[i].Node));
            }

            Debug.WriteLine("节点创建完成");

            for (int i = 0; i < num; i++)
            {
                EndCondition ec = textNodes[i].Node.EndCondition;

                for (int j = 0; j < ec.Answers.Count; j++)
                {
                    if (ec.Answers[j].Item2 == "")
                    { continue; }//直接跳过空的

                    bool found = false;
                    for (int k = 0; k < num; k++)
                    {
                        if (ec.Answers[j].Item2 == textNodes[k].Node.Name)
                        {
                            found = true;
                            //准备为nodeButton添加post
                            NodeButton.Link(nodeButtons[i], nodeButtons[k], ec.Answers[j].Item1);
                            break;
                        }
                    }
                    if (!found)
                    { ec.Answers[j] = (ec.Answers[j].Item1, "", ec.Answers[j].Item3); }
                }
            }

            Debug.WriteLine("节点链接完成");

            for (int i = 0; i < num; i++)
            {
                Canvas.SetLeft(nodeButtons[i], Math.Max(0, textNodes[i].Left));
                Canvas.SetTop(nodeButtons[i], Math.Max(0, textNodes[i].Top));
                _container.Children.Add(nodeButtons[i]);
            }

            Debug.WriteLine("节点绘制完成");

            //添加线
            //连线现在放到别的地方，即nodebutton全部loaded之后执行
            Debug.WriteLine("节点图创建完成");
            NodeCount = num;
        }

        /// <summary> 节点创建完成后，绘制线条 </summary>
        private async void DrawLines()
        {
            await _container.Dispatcher.BeginInvoke(new Action(
                delegate
                {
                    HashSet<NodeButton> nodeButtons = new HashSet<NodeButton>();
                    foreach (UIElement element in Container.Children)
                    {
                        if (element is NodeButton)
                        { nodeButtons.Add(element as NodeButton); }
                    }
                    foreach (NodeButton node in nodeButtons)
                    {
                        node.DrawPostLines();
                    }
                    CountCharWord(true);
                }), DispatcherPriority.ApplicationIdle);
            ViewModelFactory.Main.ShowWorkGrid();
        }
        #endregion

        #region 导出图片
        public async void OutputImg(string fileName)
        {
            //如果要提高清晰度，可以把下面的4个*2都改为*4
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap(((int)Container.RenderSize.Width + 10) * 2,
                ((int)Container.RenderSize.Height + 10) * 2,
                2 * 96d / Container.ScaleRatio, 2 * 96d / Container.ScaleRatio, PixelFormats.Default);
            targetBitmap.Render(Container);
            PngBitmapEncoder saveEncoder = new PngBitmapEncoder();
            saveEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));
            FileStream fs = File.Open(fileName, FileMode.OpenOrCreate);
            saveEncoder.Save(fs);

            await Task.Delay(10);
            fs.Flush();
            fs.Close();
            Process.Start("explorer.exe", Path.GetDirectoryName(fileName));
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
            if (SelectedNodes.Contains(node) && SelectedNodes.Count == 1)
            {
                SelectedNodes[0].NodeState = NodeState.Selected;
                return;
            }
            ClearSelection();
            SelectedNodes.Add(node);
            SelectedNodes[0].NodeState = NodeState.Selected;
        }
        public void NewSelection(List<NodeButton> nodes)
        {
            ClearSelection();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].NodeState = NodeState.Selected;
                SelectedNodes.Add(nodes[i]);
            }
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

        public void SelectAll()
        {
            foreach (UserControl control in Container.Children)
            {
                if (control is NodeButton)
                {
                    (control as NodeButton).NodeState = NodeState.Selected;
                    SelectedNodes.Add((control as NodeButton));
                }
            }
        }
        #endregion

        #region 节点累计、获取List、新节点名字与重复检查
        /// <summary> 获得节点数 </summary>
        public int GetNodeCount()
        {
            int i = 0;
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                { i++; }
            }
            return i;
        }

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
                        (control as NodeButton).TextNode,
                        Canvas.GetLeft(control),
                        Canvas.GetTop(control)));//注意left和top都不受scale影响
                }
            }
            Debug.WriteLine("成功取得节点与坐标信息");
            return textNodes;
        }

        /// <summary> 获得textNode列表  </summary>
        [Obsolete]
        public List<TextNode> GetTextNodeList()
        {
            List<TextNode> textNodes = new List<TextNode>();
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    textNodes.Add((control as NodeButton).TextNode);
                }
            }
            return textNodes;
        }

        /// <summary>获得一个新名字</summary>
        public string GetNewName()
        {
            //创造一个不重名的
            string newName = LanguageManager.Instance["N_NewNode"];
            int i = 1;
            bool repeated = true;
            while (repeated)
            {
                repeated = false;
                foreach (TextNode node in Nodes)
                {
                    if (node.Name == newName + i.ToString())
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
            foreach (TextNode node in Nodes)
            {
                if (node.Name == newName)
                { return true; }
            }
            return false;
        }

        public TextNode GetNodeByName(string name)
        {
            foreach (TextNode node in Nodes)
            {
                if (node.Name == name)
                { return node; }
            }
            return null;
        }
        #endregion

        #region 添加节点
        public void AddNodeButton(NodeBase preNode, NodeButton postNode, double xPos, double yPos)
        {
            NodeButton newNodeButton = new NodeButton(new TextNode(GetNewName()));
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
            _container.Children.Add(new ConnectingLine(newNodeButton, postNode));

            ViewModelFactory.Main.IsModified = true;
            NodeCount = GetNodeCount();
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
                SearchedIndex = 0;
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
            SearchedIndex++;
            if (SearchedIndex >= SearchedNodes.Count)
            { SearchedIndex = 0; }
            ScrollToNode(SearchedNodes[SearchedIndex]);
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
                    if (nb.TextNode == node)
                    {
                        SearchedNodes.Add(nb);
                        nb.NodeState = NodeState.Searched;
                        SearchedIndex = 0;
                        ScrollToNode(nb);
                        return;
                    }
                }
            }
        }
        #endregion

        #region 节点选择功能与选择新后继节点功能
        /// <summary> 进入等待点击以选择一个新的后继节点的状态 </summary>
        public void WaitClick(NodeBase waiter)
        {
            waitingNode = waiter;
            int nodeCount = 0;
            //开启等待点击
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    //开启nodebutton的上层border，等待点击其中一个
                    (control as NodeButton).UpperBd.Visibility = Visibility.Visible;
                    nodeCount++;
                }
            }
            waiter.FatherNode.UpperBd.Visibility = Visibility.Hidden;
            if (nodeCount > 1)
            { ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_WaitClick"]); }
            else
            { waitingNode = null; }
        }

        /// <summary> 新的后继节点选择完成了，传入null表示取消选择 </summary>
        public void PostNodeChoosed(NodeButton post)
        {
            foreach (UserControl control in _container.Children)
            {
                if (control is NodeButton)
                {
                    (control as NodeButton).UpperBd.Visibility = Visibility.Hidden;
                }
            }
            if (post == null)//没有选择
            {
                waitingNode = null;
                ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_Cancelled"]);
                return;
            }
            //首先断开waitNode原有的连线
            ConnectingLine cline = null;//找到原本的连线
            foreach (ConnectingLine line in waitingNode.FatherNode.PostLines)
            {
                if (line.BeginNode == waitingNode)
                { cline = line; }
            }
            if (cline != null)//不一定有这条原本连线
            {
                NodeButton.UnLink(waitingNode, false);
                cline.Delete();
            }
            //在waitNode和Post之间连线
            NodeButton.Link(waitingNode, post);
            _container.Children.Add(new ConnectingLine(waitingNode, post));
            //修改标签页
            ViewModelFactory.Main.ReLoadTab(waitingNode.FatherTextNode);
            ViewModelFactory.Main.RaiseHint(LanguageManager.Instance["Hint_ClickOver"]);
            waitingNode = null;
        }
        #endregion

        #endregion
    }
}
