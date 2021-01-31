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
        public NodeButton BeginElement { get; set; }
        /// <summary>
        /// 终止节点
        /// </summary>
        public NodeButton EndElement { get; set; }
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
            NodeButton.UnLink(BeginElement, EndElement);
            BeginElement.postLines.Remove(this);
            EndElement.preLines.Remove(this);
            //删去本线条
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).DeleteLine(this);
        }
        //添加节点
        private void addNode_Click(object sender, RoutedEventArgs e)
        {
            //断开起始点和终止点
            NodeButton.UnLink(BeginElement, EndElement);
            BeginElement.postLines.Remove(this);
            EndElement.preLines.Remove(this);
            //加入新节点，相关的link和画线都在fcc里完成
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).AddNodeButton(new TextNode(),
                BeginElement, EndElement, mousePt.X, mousePt.Y);
            //删去本线条
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).DeleteLine(this);
        }
        #endregion

        #endregion

        #region 方法
        /// <summary>
        /// 画线
        /// </summary>
        public void Drawing()
        {
            if (BeginElement == null || EndElement == null)
            { return; }
            Vector beginVec = VisualTreeHelper.GetOffset(BeginElement);
            Vector endVec = VisualTreeHelper.GetOffset(EndElement);
            Point beginPt = new Point(beginVec.X + BeginElement.ActualWidth / 2.0, beginVec.Y + BeginElement.ActualHeight / 2.0);
            Point endPt = new Point(endVec.X + EndElement.ActualWidth / 2.0, endVec.Y + EndElement.ActualHeight / 2.0);
            Point c1Pt = new Point(beginPt.X, beginPt.Y * 0.5 + endPt.Y * 0.5);
            Point c2Pt = new Point(endPt.X, beginPt.Y * 0.5 + endPt.Y * 0.5);
            //三次bezier曲线
            Path.Data = Geometry.Parse("M" + beginPt.ToString() + " C" + c1Pt.ToString() + " "
                + c2Pt.ToString() + " " + endPt.ToString());
            //更新tooltip
            Path.ToolTip = "从" + BeginElement.textNode.Name + "\n到" + EndElement.textNode.Name;
        }
        #endregion
    }
}
