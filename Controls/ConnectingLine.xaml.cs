using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultiBranchTexter
{
    /// <summary>
    /// ConnectingLine连接流程图中的两个节点
    /// </summary>
    public partial class ConnectingLine : UserControl
    {
        public ConnectingLine()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 前后lines添加，并且会刷新线条
        /// </summary>
        public ConnectingLine(NodeBase begin, NodeButton end)
        {
            InitializeComponent();
            BeginNode = begin;
            EndNode = end;
            begin.fatherNode.postLines.Add(this);
            end.preLines.Add(this);
            begin.fatherNode.UpdatePostLines();
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
        private Point beginOffset = new Point(0, 0);
        private Point endOffset = new Point(0, 0);
        #endregion

        #region 事件
        #region 自身鼠标事件
        //鼠标进入
        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            //设置显示顺序为1，以显示在其他connectingline上面
            Panel.SetZIndex(this, 1);
            Path.StrokeThickness = 4;
        }
        //鼠标离开
        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            Panel.SetZIndex(this, 0);
            Path.StrokeThickness = 3;
        }

        private void Path_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mousePt = e.GetPosition(ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this));
        }
        #endregion

        #region 右键菜单操作
        //删除线条
        private void deleteLine_Click(object sender, RoutedEventArgs e)
        {
            //断开起始点和终止点
            NodeButton.UnLink(BeginNode, EndNode);
            //删去本线条
            Delete();
        }
        //添加节点
        private void addNode_Click(object sender, RoutedEventArgs e)
        {
            //断开起始点和终止点
            NodeButton.UnLink(BeginNode, EndNode);
            //加入新节点，相关的link和画线都在fcc里完成
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).AddNodeButton(new TextNode(),
                BeginNode, EndNode, mousePt.X, mousePt.Y);
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
            Point endPt = EndNode.GetCanvasOffset() + EndNode.GetPreLineEnd(this);
            if (BeginNode.fatherNode.textNode.endCondition is MultiAnswerCondition)
            {
                //如果前驱是个多选模式节点，则特别处理
                //取得偏移点
                Point beginOffsetPt = new Point(beginPt.X + 60 + 10 * (BeginNode as NodeEndMAAnswer).GetIndex(), beginPt.Y);
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
            Path.ToolTip = "从" + BeginNode.fatherNode.textNode.Name + "\n到" + EndNode.textNode.Name;
        }

        /// <summary>
        /// 从前后两个节点删除自身，然后从容器移除自身
        /// </summary>
        public void Delete()
        {
            BeginNode.fatherNode.postLines.Remove(this);
            EndNode.preLines.Remove(this);
            BeginNode.fatherNode.UpdatePostLines();
            EndNode.UpdatePreLines();
            ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this).Children.Remove(this);
        }
        #endregion
    }
}
