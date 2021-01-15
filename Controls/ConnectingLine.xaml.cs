using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MultiBranchTexter
{
    /// <summary>
    /// ConnectingLine.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectingLine : UserControl
    {
      
        public NodeButton BeginElement { get; set; }
        public NodeButton EndElement { get; set; }

        public ConnectingLine()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 开始画线
        /// </summary>
        public void StartDrawing()
        {
            if (BeginElement == null || EndElement == null)
            { return; }
            Vector beginVec = VisualTreeHelper.GetOffset(BeginElement);
            Vector endVec = VisualTreeHelper.GetOffset(EndElement);
            Point beginPt = new Point(beginVec.X + BeginElement.ActualWidth / 2.0, beginVec.Y + BeginElement.ActualHeight /2.0);
            Point endPt = new Point(endVec.X + EndElement.ActualWidth / 2.0, endVec.Y + EndElement.ActualHeight / 2.0);
            Point c1Pt = new Point(beginPt.X, beginPt.Y * 0.64 + endPt.Y * 0.36);
            Point c2Pt = new Point(endPt.X, beginPt.Y * 0.36 + endPt.Y * 0.64);
            //三次bezier曲线
            Path.Data = Geometry.Parse("M" + beginPt.ToString() + " C" + c1Pt.ToString() + " "
                + c2Pt.ToString() + " " + endPt.ToString());
            //更新tooltip
            Path.ToolTip = "从" + BeginElement.textNode.Name + "\n到" + EndElement.textNode.Name;
        }

        //左键点击线条
        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        //右键点击线条
        private void Path_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        //鼠标进入
        private void Path_MouseEnter(object sender, MouseEventArgs e)
        {
            Path.Stroke = new SolidColorBrush(Colors.Red);
            Path.StrokeThickness = 4;
        }
        //鼠标离开
        private void Path_MouseLeave(object sender, MouseEventArgs e)
        {
            Path.Stroke = new SolidColorBrush(Colors.Aqua);
            Path.StrokeThickness = 3;
        }
    }
}
