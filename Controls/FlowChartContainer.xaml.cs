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
using MultiBranchTexter.Model;

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
            b1.AddPostNode(b2);
            b1.AddPostNode(b4);
            b2.AddPostNode(b3);
            b1.DrawLines();
            b2.DrawLines();
            //测试读取Test文件夹下的mbtxt文件
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "Test\\test.mbtxt";
            MBFileReader reader = new MBFileReader(path);
        }

      
    }
}
