﻿using System.Windows;
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
        //依赖属性注册
        public static readonly DependencyProperty BeginElementProperty =
            DependencyProperty.Register("BeginElement", typeof(FrameworkElement), typeof(ConnectingLine));
        public static readonly DependencyProperty EndElementProperty =
            DependencyProperty.Register("EndElement", typeof(FrameworkElement), typeof(ConnectingLine));

        public FrameworkElement BeginElement
        {
            get { return (FrameworkElement)GetValue(BeginElementProperty); }
            set { SetValue(BeginElementProperty, value); }
        }
        public FrameworkElement EndElement
        {
            get { return (FrameworkElement)GetValue(EndElementProperty); }
            set { SetValue(EndElementProperty, value); }
        }

        public ConnectingLine()
        {
            InitializeComponent();
        }


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
            Path.Data = Geometry.Parse("M" + beginPt.ToString() + " C" + c1Pt.ToString() + " "
                + c2Pt.ToString() + " " + endPt.ToString());
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
