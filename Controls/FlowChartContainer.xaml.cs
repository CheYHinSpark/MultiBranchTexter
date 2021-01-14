using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiBranchTexter
{
    /// <summary>
    /// FlowChartContainer.xaml 的交互逻辑
    /// </summary>
    public partial class FlowChartContainer : UserControl
    {
        public FlowChartContainer()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            line.BeginElement = b1;
            line.EndElement = b2;
            line.StartDrawing();
        }
    }
}
