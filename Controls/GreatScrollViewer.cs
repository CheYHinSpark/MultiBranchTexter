using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MultiBranchTexter.Controls
{
    public class GreatScrollViewer : ScrollViewer
    {
        private Grid scrollContentGrid;

        private DispatcherTimer timer = null;


        private Border leftBd;
        private Border topBd;
        private Border rightBd;
        private Border bottomBd;

        public GreatScrollViewer()
        {
            //PreviewMouseWheel += GreatScrollViewer_MouseWheel;
            Loaded += GreatScrollViewer_Loaded;
        }

        private void GreatScrollViewer_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            
        }

        private void GreatScrollViewer_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // 查找模板
            if (App.Current.Resources["greatScrollViewerTemplate"] is ControlTemplate greatScrollViewerTemplate)
            {
                scrollContentGrid = greatScrollViewerTemplate.FindName("scrollContentGrid", this) as Grid;
                leftBd = greatScrollViewerTemplate.FindName("leftBd", this) as Border;
                topBd = greatScrollViewerTemplate.FindName("topBd", this) as Border;
                rightBd = greatScrollViewerTemplate.FindName("rightBd", this) as Border;
                bottomBd = greatScrollViewerTemplate.FindName("bottomBd", this) as Border;

                scrollContentGrid.PreviewMouseMove += ScrollContentGrid_MouseMove;
                leftBd.Opacity = 0;
                topBd.Opacity = 0;
                rightBd.Opacity = 0;
                bottomBd.Opacity = 0;
            }
        }

        private void ScrollContentGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
        }
    }
}