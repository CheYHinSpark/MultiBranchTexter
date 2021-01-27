using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.Controls
{
    public class AutoSizeCanvas : Canvas
    {
        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            if (!base.InternalChildren.OfType<UIElement>().Any()) 
                return new Size(1, 1);
            double width = base
                .InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Width + (double)i.GetValue(Canvas.LeftProperty));

            double height = base
                .InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Height + (double)i.GetValue(Canvas.TopProperty));

            return new Size(width, height);
        }
    }
}
