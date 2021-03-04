using System;
using System.Collections.Generic;
using System.Windows;
using System.Text;
using System.Windows.Controls;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 可以设置负长宽的Border，只能被装在Canvas里面
    /// </summary>
    public class NegaBorder: Border
    {
        public Point StartPt { get; set; }

        public void SetEndPt(double ex, double ey)
        {
            Margin = new Thickness(
                Math.Min(ex,StartPt.X),Math.Min(ey,StartPt.Y),
                0,0
                );
            Width = Math.Abs(StartPt.X - ex);
            Height = Math.Abs(StartPt.Y - ey);
        }

        public Vector GetLeftTop()
        {
            return new Vector(Margin.Left, Margin.Top);
        }
        public Vector GetRightBottom()
        {
            return new Vector(Margin.Left + Width, Margin.Top + Height);
        }
    }
}
