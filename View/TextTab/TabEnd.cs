using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MultiBranchTexter.Model;
using MultiBranchTexter.Resources;
using MultiBranchTexter.ViewModel;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// TabEnd.xaml 的交互逻辑
    /// </summary>
    public class TabEnd : UserControl
    {
        //控件
        private TextBlock questionTxt;
        private Grid container;
        private Button backBtn;
        //信息
        private TextNode textNode;
       
        public TabEnd()
        {
            Template = FindResource("tabEndTemplate") as ControlTemplate;
            Loaded += TabEnd_Loaded;
        }

        #region 事件
        private void TabEnd_Loaded(object sender, RoutedEventArgs e)
        {
            questionTxt = GetTemplateChild("questionTxt") as TextBlock;
            container = GetTemplateChild("container") as Grid;
            backBtn = GetTemplateChild("backBtn") as Button;

            backBtn.Click += BackBtn_Click;

            LoadTabEnd();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.FlowChartWidth = "*";
            ViewModelFactory.FCC.LocateToNode(textNode);
        }
        #endregion


        #region 方法
        /// <summary> 重新装载tabend </summary>
        public void LoadTabEnd()
        { 
            container.Children.Clear();
            container.RowDefinitions.Clear(); //清理container
            //开始根据后继类型添加东西
            questionTxt.Text = textNode.EndCondition.Question;
            for (int i =0;i < textNode.EndCondition.Answers.Count;i++)
            {
                RowDefinition row = new RowDefinition { Height = new GridLength(20) };
                container.RowDefinitions.Add(row);
                TabEndItem tei = new TabEndItem(textNode.EndCondition.Answers[i].Item1,
                    textNode.EndCondition.Answers[i].Item2);
                tei.SetValue(Grid.RowProperty, i);//设置行
                container.Children.Add(tei);
            }
            container.UpdateLayout();
            //调整scrollviewer的最大高度
            ControlTreeHelper
                .FindParentOfType<ScrollViewer>(container)
                .MaxHeight = Math.Min(120, container.ActualHeight);
        }

        /// <summary> 设置节点 </summary>
        public void SetTabEnd(TextNode node)
        {
            textNode = node;
        }
        #endregion
    }
}
