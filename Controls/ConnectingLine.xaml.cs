using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
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
            //BeginNode.fatherNode.postLines.Remove(this);
            //EndNode.preLines.Remove(this);已经在Delete里面完成了
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
        /// 画线
        /// </summary>
        public void Drawing()
        {
            if (BeginNode == null || EndNode == null)
            { return; }
            Point beginPt = BeginNode
                .TransformToAncestor(ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(BeginNode))
                .Transform(new Point(BeginNode.ActualWidth / 2.0, BeginNode.ActualHeight / 2.0));
            Point endPt = EndNode
                .TransformToAncestor(ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(EndNode))
                .Transform(new Point(EndNode.ActualWidth / 2.0, EndNode.ActualHeight / 2.0));
            Point c1Pt = new Point(beginPt.X, beginPt.Y * 0.5 + endPt.Y * 0.5);
            Point c2Pt = new Point(endPt.X, beginPt.Y * 0.5 + endPt.Y * 0.5);
            //三次bezier曲线
            Path.Data = Geometry.Parse("M" + beginPt.ToString() + " C" + c1Pt.ToString() + " "
                + c2Pt.ToString() + " " + endPt.ToString());
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
            ControlTreeHelper.FindParentOfType<AutoSizeCanvas>(this).Children.Remove(this);
        }
        #endregion
    }
}
