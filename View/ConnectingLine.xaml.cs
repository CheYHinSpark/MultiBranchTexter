using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// ConnectingLine连接流程图中的两个节点
    /// </summary>
    public partial class ConnectingLine : UserControl
    {
        private readonly bool isBeginMA = false;//起始节点是否是一个多问题节点

        /// <summary>
        /// 前后lines添加，并且会刷新线条
        /// </summary>
        public ConnectingLine(NodeBase begin, NodeButton end)
        {
            InitializeComponent();
            BeginNode = begin;
            EndNode = end;
            begin.FatherNode.PostLines.Add(this);
            end.PreLines.Add(this);
            begin.FatherNode.UpdatePostLines();
            isBeginMA = begin.FatherTextNode.EndCondition.EndType == EndType.MultiAnswers;
            end.UpdatePreLines();
        }

        #region 成员变量
        /// <summary>
        /// 起始节点
        /// </summary>
        public NodeBase BeginNode { get; set; }
        /// <summary>
        /// 终止节点
        /// </summary>
        public NodeButton EndNode { get; set; }
        //记录鼠标位置
        private Point mousePt = new Point();
        #endregion

        #region 事件
        #region 自身鼠标事件
        //鼠标进入
        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            //设置显示顺序为1，以显示在其他connectingline上面
            Panel.SetZIndex(this, 5);
            Opacity = 0.6;
            Path.StrokeThickness = 4;
        }
        //鼠标离开
        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            Panel.SetZIndex(this, 0);
            Opacity = 1;
            Path.StrokeThickness = 3;
        }

        private void Path_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mousePt = e.GetPosition(ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this));
        }
        #endregion

        #region 右键菜单操作
        //删除线条
        private void DeleteLine_Click(object sender, RoutedEventArgs e)
        {
            //断开起始点和终止点
            NodeButton.UnLink(BeginNode);
            //删去本线条
            Delete();
        }
        //添加节点
        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            //断开起始点和终止点
            NodeButton.UnLink(BeginNode);
            //加入新节点，相关的link和画线都在fcc里完成
            ViewModelFactory.FCC.AddNodeButton(BeginNode, EndNode, mousePt.X, mousePt.Y);
            //删去本线条
            Delete();
        }
        #endregion

        #endregion

        #region 方法
        /// <summary>
        /// 画线，现在该方法已经不需要线条本身在容器里，但是前后节点必须已经在容器里面
        /// </summary>
        public void Update()
        {
            if (BeginNode == null || EndNode == null)
            { return; }
            //获取起始点
            Point beginPt = BeginNode.GetCanvasOffset() + new Vector(BeginNode.ActualWidth / 2.0, BeginNode.ActualHeight / 2.0);
            Point endPt = EndNode.GetCanvasOffset() + EndNode.GetPreLineEndOffset(this);
            if (isBeginMA)
            {
                //如果前驱是个多选模式节点，则特别处理
                //取得偏移点
                Point beginOffsetPt = new Point(beginPt.X + BeginNode.ActualWidth / 2.0 + 10 + 10 * (BeginNode as NodeEndMAAnswer).GetIndex(), beginPt.Y);
                Point c1Pt, c2Pt;
                double xt = endPt.X - beginOffsetPt.X;
                double yt = endPt.Y - beginOffsetPt.Y;
                if (Math.Abs(xt) + 80 - Math.Abs(yt) > 0)
                {
                    //这表示横向差距较大
                    c1Pt = new Point(beginOffsetPt.X, beginOffsetPt.Y * 0.5 + endPt.Y * 0.5);
                    c2Pt = new Point(endPt.X, beginOffsetPt.Y * 0.5 + endPt.Y * 0.5);
                }
                else//这表示横向差距不够大
                {
                    c1Pt = new Point(beginOffsetPt.X,
                        beginOffsetPt.Y * 0.5 + endPt.Y * 0.5 - Math.Sign(yt) * Math.Abs(xt / 2));
                    c2Pt = new Point(endPt.X,
                        beginOffsetPt.Y * 0.5 + endPt.Y * 0.5 + Math.Sign(yt) * Math.Abs(xt / 2));
                }
                Path.Data = Geometry.Parse("M" + beginPt.ToString() + " " 
                    + beginOffsetPt.ToString() + " " + c1Pt.ToString() + " "
                    + c2Pt.ToString() + " L" + endPt.ToString());
            }
            else
            {
                Point c1Pt, c2Pt;
                double xt = endPt.X - beginPt.X;
                double yt = endPt.Y - beginPt.Y;
                if (Math.Abs(xt) + 80 - Math.Abs(yt) > 0)
                {
                    //这表示横向差距较大
                    c1Pt = new Point(beginPt.X, beginPt.Y * 0.5 + endPt.Y * 0.5);
                    c2Pt = new Point(endPt.X, beginPt.Y * 0.5 + endPt.Y * 0.5);
                    
                }
                else//这表示横向差距不够大
                {
                    c1Pt = new Point(beginPt.X, 
                        beginPt.Y * 0.5 + endPt.Y * 0.5 - Math.Sign(yt) * Math.Abs(xt / 2));
                    c2Pt = new Point(endPt.X, 
                        beginPt.Y * 0.5 + endPt.Y * 0.5 + Math.Sign(yt) * Math.Abs(xt / 2));
                }
                Path.Data = Geometry.Parse("M" + beginPt.ToString() + " " + c1Pt.ToString() + " "
                    + c2Pt.ToString() + " L" + endPt.ToString());
            }
            //更新tooltip
            Path.ToolTip = "从 " + BeginNode.FatherTextNode.Name + "\n到 " + EndNode.TextNode.Name;
            //如果后继在上方，调整颜色
            if (beginPt.Y > endPt.Y)
            { Path.Tag = "1"; }
            else { Path.Tag = "0"; }
        }

        /// <summary>
        /// 从前后两个节点删除自身，然后从容器移除自身
        /// </summary>
        public void Delete()
        {
            BeginNode.FatherNode.PostLines.Remove(this);
            EndNode.PreLines.Remove(this);
            BeginNode.FatherNode.UpdatePostLines();
            EndNode.UpdatePreLines();
            ViewModelFactory.Main.IsModified = true;
            ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this).Children.Remove(this);
            Debug.WriteLine("成功删除连线");
        }
        #endregion
    }
}
