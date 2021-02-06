using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// TabEnd.xaml 的交互逻辑
    /// </summary>
    public partial class TabEnd : UserControl
    {
        private TextNode textNode;
        public TextNode nextNode;
       
        public TabEnd()
        {
            InitializeComponent();
        }
        public TabEnd(TextNode newNode)
        {
            InitializeComponent();
            SetTabEnd(newNode);
        }

        /// <summary>
        /// 输入一个节点，根据其结尾信息生成自身
        /// </summary>
        public void SetTabEnd(TextNode node)
        {
            textNode = node;
            questionTxt.Text = "";
            container.Children.Clear();
            container.RowDefinitions.Clear();//清理container
            //开始根据后继类型
            if (textNode.endCondition == null)
            { 
                //表示是单一后继或者无后继
                if (textNode.PostNodes.Count > 0)
                {
                    RowDefinition row = new RowDefinition { Height = new GridLength(20) };
                    container.RowDefinitions.Add(row);
                    TabEndItem tei = new TabEndItem(textNode, "", textNode.PostNodes[0]);
                    tei.SetValue(Grid.RowProperty, 0);
                    container.Children.Add(tei);
                }
            }
            else if (textNode.endCondition is YesNoCondition)
            {
                questionTxt.Text = (textNode.endCondition as YesNoCondition).Question;
                TextNode yes = (textNode.endCondition as YesNoCondition).YesNode;
                TextNode no = (textNode.endCondition as YesNoCondition).NoNode;
                RowDefinition row1 = new RowDefinition { Height = new GridLength(20) };
                container.RowDefinitions.Add(row1);
                TabEndItem ytei = new TabEndItem(textNode, "是", yes);
                ytei.SetValue(Grid.RowProperty, 0);
                container.Children.Add(ytei);
                RowDefinition row2 = new RowDefinition { Height = new GridLength(20) };
                container.RowDefinitions.Add(row2);
                TabEndItem ntei = new TabEndItem(textNode, "否", no);
                ntei.SetValue(Grid.RowProperty, 1);
                container.Children.Add(ntei);
            }
            else
            {
                questionTxt.Text = (textNode.endCondition as MultiAnswerCondition).Question;
                List<AnswerToNode> atns = (textNode.endCondition as MultiAnswerCondition).AnswerToNodes;
                for (int i =0;i<atns.Count;i++)
                {
                    RowDefinition row = new RowDefinition { Height = new GridLength(20) };
                    container.RowDefinitions.Add(row);
                    TabEndItem tei = new TabEndItem(textNode, atns[i].Answer, atns[i].PostNode);
                    tei.SetValue(Grid.RowProperty, i);
                    container.Children.Add(tei);
                }
            }
            container.UpdateLayout();
            Debug.WriteLine(container.RowDefinitions.Count);
        }
    }
}
