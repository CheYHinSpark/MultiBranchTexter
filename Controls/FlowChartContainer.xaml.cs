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
            //测试读取Test文件夹下的mbtxt文件
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "Test\\test.mbtxt";
            MBFileReader reader = new MBFileReader(path);
            //reader.Read();
            UpdateControls(reader.Read());
        }

        //根据List<TextNode>建立树状图
        //不处理循坏连接的情况
        //复杂情况可能出问题
        private void UpdateControls(List<TextNode> textNodes)
        {
            List<bool> hasCreated = new List<bool>();
            List<int> groupIndexOfBtns = new List<int>();
            List<NodeButton> nodeButtons = new List<NodeButton>();
            //二维List，对NodeButton分组
            List<List<NodeButton>> groupedNodes = new List<List<NodeButton>>
            {
                new List<NodeButton>()
            };
            //初始化hasCreated
            for (int i = 0; i<textNodes.Count;i++)
            {
                hasCreated.Add(false);
                groupIndexOfBtns.Add(0);
                nodeButtons.Add(new NodeButton(textNodes[i]));
            }
            for (int i = 0; i < textNodes.Count;i++)
            {
                //对这个Node，将其所有postNode的组数变为其当前组数和本Node组数+1的较大值
                List<int> vs = textNodes[i].GetPostNodeIndex(textNodes);
                for (int j = 0; j < vs.Count;j++)
                {
                    groupIndexOfBtns[vs[j]] = groupIndexOfBtns[vs[j]] >= groupIndexOfBtns[i] + 1 ? groupIndexOfBtns[vs[j]] : groupIndexOfBtns[i] + 1;
                    //为nodeButton添加post
                    nodeButtons[i].AddPostNode(nodeButtons[vs[j]]);
                }
            }
            //根据Index分组
            for (int i = 0; i < textNodes.Count; i++)
            {
                //如果groupedNodes组数不够，先扩充
                for (int j = 0; j <= groupIndexOfBtns[i] - groupedNodes.Count; j++)
                {
                    groupedNodes.Add(new List<NodeButton>());
                }
                groupedNodes[groupIndexOfBtns[i]].Add(nodeButtons[i]);
            }
            //根据分组开始NodeButton
            for (int i = 0; i < groupedNodes.Count;i++)
            {
                for (int j = 0;j < groupedNodes[i].Count;j++)
                {
                    groupedNodes[i][j].Margin = new Thickness(40 + 100 * j, 60 + 160 * i, 0, 0);
                    container.Children.Add(groupedNodes[i][j]);
                }
            }

            for (int i = 0; i < textNodes.Count; i++)
            {
                nodeButtons[i].DrawPostLines(container);
            }
        }
    }
}
