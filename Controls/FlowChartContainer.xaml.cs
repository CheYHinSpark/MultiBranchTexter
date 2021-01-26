using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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
            //测试读取Test文件夹下的mbtxt文件
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "Test\\test.mbtxt";
            MBFileReader reader = new MBFileReader(path);
            //reader.Read();
            DrawFlowChart(reader.Read());
        }

        //根据List<TextNode>建立树状图
        //不处理循坏连接的情况
        //有向无环图排序
        private void DrawFlowChart(List<TextNode> textNodes)
        {
            //nodeButtons总表
            List<NodeButton> nodeButtons = new List<NodeButton>();

            int num = textNodes.Count;
            //node所在组数，实际是画图后的行数
            int[] groupIndexOfBtns = new int[num];
            //二维List，对NodeButton分组
            List<List<NodeButton>> groupedNodes = new List<List<NodeButton>>();
            //是否完成数组
            bool[] hasDone = new bool[num];
            //已完成的node指标
            List<int> hasDoneIndex = new List<int>();
            //初始化矩阵
            bool[,] mat = new bool[num, num];
            //初始化
            for (int i = 0; i < num; i++)
            {
                //初始化button
                nodeButtons.Add(new NodeButton(textNodes[i]));
                //初始化矩阵
                for (int j = 0; j < num; j++)
                {
                    mat[i, j] = false;
                }
                //一开始都未完成
                hasDone[i] = false;
                //初始化组数
                groupIndexOfBtns[i] = 0;
                
            }
            for (int i = 0; i < textNodes.Count; i++)
            {
                List<int> postIndex = textNodes[i].GetPostNodeIndex(textNodes);
                //为矩阵赋值，如果i有j作为后继，则ij位置为真
                for (int j = 0; j < postIndex.Count; j++)
                {
                    mat[i, postIndex[j]] = true;
                    //为nodeButton添加post
                    nodeButtons[i].AddPostNode(nodeButtons[postIndex[j]]);
                }
            }
            while (hasDoneIndex.Count < num)
            {
                //搜寻尚未完成的node中无前驱者
                List<NodeButton> noPreNodes = new List<NodeButton>();
                for (int j = 0; j < num; j++)
                {
                    //已经做了，跳过
                    if (hasDoneIndex.Contains(j))
                    { continue; }
                    bool shouldAdd = true;
                    for (int i = 0; i < num;i++)
                    {
                        //找到一个前驱，跳过
                        if (mat[i,j])
                        {
                            shouldAdd = false;
                            break;
                        }
                    }
                    if (!shouldAdd)
                    { continue; }
                    //未找到前驱
                    noPreNodes.Add(nodeButtons[j]);
                    hasDoneIndex.Add(j);
                }
                //添加
                groupedNodes.Add(noPreNodes);
                //处理身后事
                for (int i = 0; i< hasDoneIndex.Count; i++)
                {
                    for (int j = 0; j < num; j++)
                    {
                        mat[hasDoneIndex[i], j] = false;
                    }
                }
            }
            //根据分组开始放置NodeButton
            for (int i = 0; i < groupedNodes.Count; i++)
            {
                for (int j = 0; j < groupedNodes[i].Count; j++)
                {
                    groupedNodes[i][j].Margin = new Thickness(40 + 120 * j, 60 + 160 * i, 0, 0);
                    container.Children.Add(groupedNodes[i][j]);
                }
            }
            //连线
            for (int i = 0; i < textNodes.Count; i++)
            {
                nodeButtons[i].DrawPostLines(container);
            }
        }

        //删除连线
        public void DeleteLine(ConnectingLine line)
        {
            container.Children.Remove(line);
        }
        /// <summary>
        /// 获得textNode列表
        /// </summary>
        public List<TextNode> GetTextNodes()
        {
            List<TextNode> textNodes = new List<TextNode>();
            foreach (UserControl control in container.Children)
            {
                if (control is NodeButton)
                {
                    textNodes.Add((control as NodeButton).textNode);
                }
            }
            return textNodes;
        }
        /// <summary>
        /// 添加节点并更新流程图
        /// </summary>
        /// <param name="newNode"></param>
        public void AddNodeAndUpdateFlowChart(TextNode newNode)
        {
            List<TextNode> textNodes = GetTextNodes();
            textNodes.Add(newNode);
            container.Children.Clear();
            DrawFlowChart(textNodes);
        }

        private void scrollViewer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source is ScrollViewer)
            {
                if (e.OriginalSource is Grid)
                {
                    //现在点击到的是空白区域，取消选中的控件
                    
                }
            }
        }
    }
}
