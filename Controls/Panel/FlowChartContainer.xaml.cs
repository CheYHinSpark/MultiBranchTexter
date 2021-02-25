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
        //与拖拽、移动有关的变量
        private Point _clickPoint;
        //等待选择后继节点的node
        private NodeBase waitingNode;
        public bool IsWaiting { get { return waitingNode != null; } }

        private readonly FCCViewModel _viewModel;

        public FlowChartContainer()
        {
            InitializeComponent();
            ViewModelFactory.SetViewModel(typeof(FCCViewModel), this.DataContext as FCCViewModel);
            _viewModel = DataContext as FCCViewModel;
            _viewModel.Container = container;
            _viewModel.ScrollViewer = scrollViewer;
        }

        #region 事件
        //滚轮事件
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
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
        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
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

                    for (int i = 0; i < _viewModel.SelectedNodes.Count; i++)
                    {
                        _viewModel.SelectedNodes[i].Move(x / container.ScaleRatio, y / container.ScaleRatio);
                    }

                    _clickPoint = e.GetPosition((ScrollViewer)sender);

                    ViewModelFactory.Main.IsModified = "*";
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
        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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
                    //如果没有打开搜索框，取消搜索
                    if (_viewModel.SearchBoxVisibility == Visibility.Hidden)
                    { _viewModel.ClearSearch(); }
                    stateHint.Text = "";
                    //准备拖拽
                    this.Cursor = Cursors.Hand;
                }
            }
        }

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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
                ViewModelFactory.FCC.NewSelection(newSelect);
            }
        }
        #endregion

        #region 节点选择功能与选择新后继节点功能
        /// <summary> 进入等待点击以选择一个新的后继节点的状态 </summary>
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
            waiter.FatherNode.UpperBd.Visibility = Visibility.Hidden;
        }

        /// <summary> 新的后继节点选择完成了，传入null表示取消选择 </summary>
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
            foreach (ConnectingLine line in waitingNode.FatherNode.postLines)
            {
                if (line.BeginNode == waitingNode)
                { cline = line; }
            }
            if (cline != null)//不一定有这条原本连线
            {
                NodeButton.UnLink(waitingNode, cline.EndNode);
                waitingNode.FatherNode.postLines.Remove(cline);
                cline.EndNode.preLines.Remove(cline);
                container.Children.Remove(cline);
            }
            //在waitNode和Post之间连线
            NodeButton.Link(waitingNode, post);
            ConnectingLine cl = new ConnectingLine(waitingNode, post);
            container.Children.Add(cl);
            //修改标签页
            ViewModelFactory.Main.ReLoadTab(waitingNode.FatherTextNode);
            waitingNode = null;
        }
        #endregion
    }
}