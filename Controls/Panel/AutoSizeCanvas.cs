using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 可以自动缩放大小的Canvas
    /// </summary>
    public class AutoSizeCanvas : Canvas
    {
        public static DependencyProperty ScaleRatioProperty =
            DependencyProperty.Register("ScaleRatio", //属性名称
                typeof(double), //属性类型
                typeof(AutoSizeCanvas), //该属性所有者，即将该属性注册到那个类上
                new PropertyMetadata(1.0)//属性默认值
                );
        
        public double ScaleRatio
        {
            get { return (double)GetValue(ScaleRatioProperty); }
            set { SetValue(ScaleRatioProperty, value); }
        }

        public bool IsResizing = false;
        private Size oldSize;

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            if (!base.InternalChildren.OfType<UIElement>().Any()) 
                return new Size(100, 100);
            double width, height;
            if (IsResizing)//这是为了优化总大小缩小时的操作手感
            {
                width = base
                .InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Width + (double)i.GetValue(LeftProperty));

                height = base
                    .InternalChildren
                    .OfType<UIElement>()
                    .Max(i => i.DesiredSize.Height + (double)i.GetValue(TopProperty));
                width = Math.Max(width, oldSize.Width);
                height = Math.Max(height, oldSize.Height);
                oldSize = new Size(width, height);
                return oldSize;
            }
            width = base
                .InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Width + (double)i.GetValue(LeftProperty));

            height = base
                .InternalChildren
                .OfType<UIElement>()
                .Max(i => i.DesiredSize.Height + (double)i.GetValue(TopProperty));

            oldSize = new Size(width, height);
            return oldSize;
        }
    }
}
