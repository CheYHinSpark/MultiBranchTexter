﻿using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
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
        private NodeBase endNode;//这个只是挂在屁股下面的那个容器里的东西

        public TextNode textNode;
        public Dictionary<string, NodeButton> answerToNodes = new Dictionary<string, NodeButton>();

        public List<ConnectingLine> postLines = new List<ConnectingLine>();
        public List<ConnectingLine> preLines = new List<ConnectingLine>();

        private NodeState _nodeState;
        public NodeState NodeState
        {
            get { return _nodeState; }
            set 
            {
                _nodeState = value;
                bgBorder.Tag = value.ToString();//使用tag来控制其样式
            }
        }

        // 移动相关
        private Point oldPoint = new Point();
        private bool isMoving = false;
        public bool IsMoving { get { return isMoving; } }

        public NodeButton(TextNode newNode)
        {
            InitializeComponent();
            textNode = newNode;
            FatherNode = this;
            ShowEndCondition();
        }
        #region 事件
        //加载完成
        private async void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            endContainer.Child = endNode;
            //显示标题
            titleBox.Text = textNode.Name;

            Panel.SetZIndex(this, 2);
            Debug.WriteLine("节点成功生成" + textNode.Name);
            await Task.Delay(10);//这是为了能让线条的位置正确
            DrawPostLines(ViewModelFactory.FCC.Container);
        }

        #region 移动与选中事件
        private void NodeButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoving)
            {
                if (e.LeftButton == MouseButtonState.Released)
                {
                    SwitchMoving();
                    return;
                }
                //移动自身位置
                Move(e.GetPosition(ViewModelFactory.FCC.Container).X - oldPoint.X,
                    e.GetPosition(ViewModelFactory.FCC.Container).Y - oldPoint.Y);
                oldPoint = e.GetPosition(ViewModelFactory.FCC.Container);
            }
        }

        private void NodeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NodeState = NodeState.Selected;
            if (e.OriginalSource is Border)
            {
                SwitchMoving();
                //通知flowchart改变selectedNodes
                ViewModelFactory.FCC.NewSelection(this);
            }
            oldPoint = e.GetPosition(ViewModelFactory.FCC.Container);
        }

        private void NodeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SwitchMoving();
        }

        private void NodeButton_MouseWheel(object sender, MouseWheelEventArgs e)
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
            ViewModelFactory.Main.OpenMBTabItem(textNode);
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
                if (ViewModelFactory.FCC.CheckRepeat(titleBox.Text))
                {
                    titleBox.Text = textNode.Name;
                    MessageBox.Show("节点名称重复，已还原");
                }
                else
                {
                    //没有重复，完成修改
                    textNode.Name = titleBox.Text;
                    //通知窗体改变相应的标签页
                    ViewModelFactory.Main.ReLoadTab(textNode);
                }
            }
            UpdateLines();
        }

        //上层bd被点击，这是在重新选择后继节点时可以被选中
        private void UpperBd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //通知流程图容器自己被选中
            ViewModelFactory.FCC.PostNodeChoosed(this);
        }

        //虽然0次引用，但是这是有用的，这是单一后继节点模式下重新选择后继节点功能
        private void NodeBase_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border)
            {
                //这样，就表示点击到了主题border上
                if ((e.OriginalSource as Border).Name == "bgBorder" && textNode.endCondition.EndType == EndType.Single)
                {
                    if (ViewModelFactory.FCC.IsWaiting)
                    { return; }
                    //进入选择模式
                    ViewModelFactory.FCC.WaitClick(this);
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
            ViewModelFactory.FCC.NodeCount = ViewModelFactory.FCC.GetNodeCount();
        }

        private void ChangeEnd_Click(object sender, RoutedEventArgs e)
        {
            string header = (string)(sender as MenuItem).Header;
            if ((header == "单一后继" && textNode.endCondition.EndType == EndType.Single)
                || (header == "判断后继" && textNode.endCondition.EndType == EndType.YesNo)
                || (header == "多选后继" && textNode.endCondition.EndType == EndType.MultiAnswers))
            { return; }
            bool needWarn = true;
            if (textNode.endCondition.EndType == EndType.Single)
            { needWarn = answerToNodes[""] != null; }
            else if (textNode.endCondition.EndType == EndType.YesNo)
            { needWarn = answerToNodes["yes"] != null || answerToNodes["no"] != null; }
            if (needWarn)
            {
                MessageBoxResult warnResult = MessageBox.Show
                    (
                    Application.Current.MainWindow,
                    "确定要变更后继条件吗？\n这将同时断开节点的所有后继连接线以及与所有后继节点的连接。\n此操作不可撤销！",
                    "警告",
                    MessageBoxButton.YesNo
                    );
                if (warnResult == MessageBoxResult.No)
                { return; }
            }
            UnLinkAllPost();//清除后继
            if (header == "单一后继")
            { textNode.endCondition = new EndCondition(EndType.Single); }
            else if (header == "判断后继")
            { textNode.endCondition = new EndCondition(EndType.YesNo); }
            else
            { textNode.endCondition = new EndCondition(EndType.MultiAnswers); }
            ShowEndCondition();
            endContainer.Child = endNode;
            //通知窗体把对应的标签改了
            ViewModelFactory.Main.IsModified = true;
            ViewModelFactory.Main.ReLoadTab(textNode);
        }
        #endregion

        #endregion

        #region 方法

        #region 静态方法
        /// <summary>
        /// 连接，注意不能在对键值的遍历中搞这个
        /// </summary>
        public static void Link(NodeButton pre, NodeButton post,string answer)
        {
            //添加
            if (pre.answerToNodes.ContainsKey(answer))
            { pre.answerToNodes[answer] = post; }
            else
            { pre.answerToNodes.Add(answer, post); }
            TextNode.Link(pre.textNode, post.textNode, answer);
        }

        /// <summary>
        /// 断开，注意不能在对键值的遍历中搞这个
        /// </summary>
        public static void UnLink(NodeButton pre, NodeButton post,string answer)
        {
            //移除
            if (pre.answerToNodes.ContainsKey(answer))
            { pre.answerToNodes[answer] = null; }
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
                    Link(pre.FatherNode, post, ""); 
                    break;
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
        /// 显示后继条件框框
        /// </summary>
        private void ShowEndCondition()
        {
            FatherNode = this;
            answerToNodes.Clear();
            switch (textNode.endCondition.EndType)
            {
                case EndType.Single:
                    //把可能存在的东西删掉
                    endNode = null;
                    answerToNodes.Add("", null);
                    break;
                case EndType.YesNo:
                    endNode = new NodeEndYesNo(textNode.endCondition);
                    (endNode as NodeEndYesNo).SetFather(this);
                    answerToNodes.Add("yes", null);
                    answerToNodes.Add("no", null);
                    break;
                case EndType.MultiAnswers:
                    endNode = new NodeEndMA(textNode.endCondition);
                    (endNode as NodeEndMA).SetFather(this);//在这个里面会新建answerToNode键值
                    break;
            }
            UpdateLayout();
        }


        /// <summary>
        /// 根据textNode的连接情况在自己和后续节点间生成连线
        /// </summary>
        public void DrawPostLines(Panel container)
        {
            switch (textNode.endCondition.EndType)
            {
                case EndType.Single:
                    if (answerToNodes[""] == null)//可能没有
                    { return; }
                    ConnectingLine line = new ConnectingLine(this, answerToNodes[""]);
                    container.Children.Add(line);
                    break;
                case EndType.YesNo:
                    if (answerToNodes["yes"] != null)
                    {
                        container.Children.Add(
                            new ConnectingLine((endNode as NodeEndYesNo).yesNode, answerToNodes["yes"])
                            );
                    }
                    if (answerToNodes["no"] != null)
                    {
                        container.Children.Add(
                            new ConnectingLine((endNode as NodeEndYesNo).noNode, answerToNodes["no"])
                            );
                    }
                    break;

                case EndType.MultiAnswers:
                    NodeEndMA tempNode = endNode as NodeEndMA;
                    foreach (UserControl control in tempNode.answerContainer.Children)
                    {
                        string answer = (control as NodeEndMAAnswer).Answer;
                        if (answerToNodes[answer] != null)
                        {
                            container.Children.Add(new ConnectingLine(control as NodeBase, answerToNodes[answer]));
                        }
                    }
                    break;
            }
        }

        /// <summary> 刷新连线 </summary>
        public void UpdateLines()
        {
            UpdatePostLines();
            UpdatePreLines();
        }

        /// <summary> 刷新前驱线 </summary>
        public void UpdatePreLines()
        {
            for (int i = 0; i < preLines.Count; i++)
            {
                preLines[i].Update();
            }
        }

        /// <summary> 刷新后继线 </summary>
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
        { return new Vector(Canvas.GetLeft(this) + this.ActualHeight / 2.0, Canvas.GetTop(this) + 25); }
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
                ViewModelFactory.FCC.Container.IsResizing = false;
                isMoving = false;
                return;
            }
            Panel.SetZIndex(this, 3);
            ViewModelFactory.FCC.Container.IsResizing = true;
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
            ViewModelFactory.Main.IsModified = true;
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
            ViewModelFactory.Main.DeleteTab(textNode);
            ViewModelFactory.Main.IsModified = true;
            //删掉自己
            ViewModelFactory.FCC.Container.Children.Remove(this);
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
            for (int i = 0; i < preLines.Count; i++)
            {
                if (preLines[i] == line)
                { return new Vector(this.ActualHeight + UpperBd.ActualWidth * ((i + 1.0) / (preLines.Count + 1.0) - 0.5), 25); }
            }
            return new Vector(this.ActualWidth / 2.0, 25);
        }

        /// <summary>
        /// 替换多选的键名
        /// </summary>
        public void ChangeAnswer(string oldKey, string newKey)
        {
            answerToNodes.Add(newKey, answerToNodes[oldKey]);
            answerToNodes.Remove(oldKey);
            textNode.endCondition.Answers.Add(newKey, textNode.endCondition.Answers[oldKey]);
            textNode.endCondition.Answers.Remove(oldKey);
        }
        #endregion
    }
}