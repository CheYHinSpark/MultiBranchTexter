using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 由于自定义了里面的scrollviewer，必须自己实现相关功能的TextBox
    /// </summary>
    public class MBTextBox:TextBox
    {
        //自己的滚轮容器
        private ScrollViewer sv;
        private double _scrollY;
        // 竖向位移
        private double ScrollY
        {
            get { return _scrollY; }
            set
            {
                _scrollY = value;
                // 开启滚动
                if (_scrollY != 0 && (task == null || task.IsCompleted))
                { task = Task.Run(ScrollYMethod); }
            }
        }

        // 滚动task
        private Task task;

        public MBTextBox()
        {
            Loaded += MBTextBox_Loaded;
        }

        private void MBTextBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            sv = GetTemplateChild("PART_ContentHost") as ScrollViewer;
            sv.MouseWheel += PART_ContentHost_MouseWheel;

            SelectionChanged += TextBox_SelectionChanged;
            TextChanged += TextBox_TextChanged;
            //Binding binding = new Binding
            //{
            //    Source = ControlTreeHelper.FindParentOfType<MainWindow>(this),
            //    Path = TextFontSize
            //};
            //SetBinding(FontSizeProperty,binding);
        }

        private void PART_ContentHost_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            sv.ScrollToVerticalOffset(sv.VerticalOffset - (e.Delta >> 2));
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var point = System.Windows.Input.Mouse.GetPosition(sv);
            // 纵向位移
            double y = point.Y;
            if (y > 0)
            {
                y -= sv.ActualHeight;
                if (y < 0) { y = 0; }
            }
            ScrollY = y;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Rect rect = GetRectFromCharacterIndex(CaretIndex);
            if (rect.Bottom < 0)
            { sv.ScrollToVerticalOffset(rect.Top + sv.ContentVerticalOffset - 2); }
            else if (rect.Top > ActualHeight)
            { sv.ScrollToVerticalOffset(rect.Bottom - ActualHeight + sv.ContentVerticalOffset + 2); }
        }

        // 竖向滚动方法
        private void ScrollYMethod()
        {
            double endOffset = 0;
            if (ScrollY < 0)       // 向上滚动
            { endOffset = 0; }
            else                   // 向下滚动
            { sv.Dispatcher.Invoke((Action)(() => endOffset = sv.ScrollableHeight), null); }
            // 初始位置
            double offset = 0;
            sv.Dispatcher.Invoke((Action)(() => offset = sv.VerticalOffset), null);
            // 开始滚动
            while (offset != endOffset && ScrollY != 0)
            {
                sv.Dispatcher.Invoke((Action)(() =>
                {
                    offset = sv.VerticalOffset;
                    sv.ScrollToVerticalOffset(sv.VerticalOffset + ScrollY);
                }), null);
                Thread.Sleep(100);
            }
        }
    }
}
