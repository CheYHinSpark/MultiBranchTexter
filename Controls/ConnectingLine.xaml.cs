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
        /// <summary>
        /// 是否被选中
        /// </summary>
        private bool isSelected = false;
        #endregion

        #region 事件
        #region 自身鼠标事件
        //鼠标进入
        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            if (isSelected)
                return;
            //设置显示顺序为1，以显示在其他connectingline上面
            Panel.SetZIndex(this, 1);
            Path.Stroke = new SolidColorBrush(Colors.Orange);
            Path.StrokeThickness = 4;
        }
        //鼠标离开
        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isSelected)
                return;
            Panel.SetZIndex(this, 0);
            Path.Stroke = new SolidColorBrush(Colors.Aqua);
            Path.StrokeThickness = 3;
        }
        #endregion

        #region 右键菜单操作
        //删除线条
        private void deleteLine_Click(object sender, RoutedEventArgs e)
        {
            //从起始点中删去终止点
            BeginElement.DeletePostNode(EndElement);
            //从终止点中删去起始点
            EndElement.DeletePostNode(BeginElement);
            //删去本线条
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).DeleteLine(this);
        }
        //添加节点
        private void addNode_Click(object sender, RoutedEventArgs e)
        {
            //从起始点中删去终止点
            BeginElement.textNode.DeletePostNode(EndElement.textNode);
            //从终止点中删去起始点
            EndElement.textNode.DeletePreNode(BeginElement.textNode);
            //添加新节点进去
            TextNode newNode = new TextNode();
            BeginElement.textNode.AddPostNode(newNode);
            newNode.AddPreNode(BeginElement.textNode);
            EndElement.textNode.AddPreNode(newNode);
            newNode.AddPostNode(EndElement.textNode);
            //更新排列
            ControlTreeHelper.FindParentOfType<FlowChartContainer>(this).AddNodeAndUpdateFlowChart(newNode);
        }
        #endregion

        #endregion

        #region 方法
        /// <summary>
        /// 开始画线
        /// </summary>
        public void StartDrawing()
        {
            if (BeginElement == null || EndElement == null)
            { return; }
            Vector beginVec = VisualTreeHelper.GetOffset(BeginElement);
            Vector endVec = VisualTreeHelper.GetOffset(EndElement);
            Point beginPt = new Point(beginVec.X + BeginElement.ActualWidth / 2.0, beginVec.Y + BeginElement.ActualHeight / 2.0);
            Point endPt = new Point(endVec.X + EndElement.ActualWidth / 2.0, endVec.Y + EndElement.ActualHeight / 2.0);
            Point c1Pt = new Point(beginPt.X, beginPt.Y * 0.64 + endPt.Y * 0.36);
            Point c2Pt = new Point(endPt.X, beginPt.Y * 0.36 + endPt.Y * 0.64);
            //三次bezier曲线
            Path.Data = Geometry.Parse("M" + beginPt.ToString() + " C" + c1Pt.ToString() + " "
                + c2Pt.ToString() + " " + endPt.ToString());
            //更新tooltip
            Path.ToolTip = "从" + BeginElement.textNode.Name + "\n到" + EndElement.textNode.Name;
        }
        #endregion

    }
}
