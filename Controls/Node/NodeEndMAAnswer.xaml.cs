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

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 多选后继节点的单个答案节点
    /// </summary>
    public partial class NodeEndMAAnswer : NodeBase
    {
        private Button maCloseBtn;
        private TextBox answerTxt;
        private AnswerToNode answerToNode = new AnswerToNode();
        /// <summary>
        /// 获取或者设置回答文本
        /// </summary>
        public string Answer
        {
            get
            { return answerTxt.Text; }
            set
            { answerTxt.Text = value; }
        }
        public NodeEndMAAnswer()
        {
            InitializeComponent();
        }
        public NodeEndMAAnswer(AnswerToNode atn)
        {
            InitializeComponent();
            answerToNode = atn;
        }
        #region 事件
        //加载完成
        private void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            maCloseBtn = GetTemplateChild("maCloseBtn") as Button;
            answerTxt = GetTemplateChild("answerTxt") as TextBox;

            maCloseBtn.Click += MaCloseBtn_Click;
            answerTxt.LostFocus += AnswerTxt_LostFocus;
            MouseDoubleClick += NodeEndMAAnswer_MouseDoubleClick;

            answerTxt.Text = answerToNode.Answer;
        }

        private void NodeEndMAAnswer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border)
            {
                //这样，就表示点击到了主题border上
                FlowChartContainer parent = ControlTreeHelper.FindParentOfType<FlowChartContainer>(this);
                if (parent.IsWaiting)
                { return; }
                //进入选择模式
                parent.WaitClick(this);
            }
        }

        //answerTXT完成修改时，检查是否出现重复
        private void AnswerTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ControlTreeHelper.FindParentOfType<NodeEndMA>(this).CheckRepeatedAnswer(Answer))
            { 
                answerTxt.Text = answerToNode.Answer;
                MessageBox.Show("回答出现重复，已还原");
            }
            else
            {
                answerToNode.Answer = answerTxt.Text;
                //通知标签页改变
                ControlTreeHelper.FindParentOfType<MainWindow>(fatherNode).ReLoadTab(fatherNode.textNode);
            }
            answerTxt.SelectionStart = 0;
        }

        private void MaCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //如果FCC处于等待选择新的后继节点状态，需要取消
            FlowChartContainer fcc = ControlTreeHelper.FindParentOfType<FlowChartContainer>(fatherNode);
            fcc.PostNodeChoosed(null);
            //断开连接
            ConnectingLine line = null;
            for (int i = 0;i < fatherNode.postLines.Count;i++)
            {
                if (fatherNode.postLines[i].BeginNode == this)
                {
                    line = fatherNode.postLines[i];
                }
            }
            //可能没有连线
            if (line != null)
            {
                NodeButton.UnLink(this, line.EndNode);
                line.Delete();
            }
            //移除自身
            ControlTreeHelper.FindParentOfType<StackPanel>(this).Children.Remove(this);
        }
        #endregion

        /// <summary>
        /// 获得在StackPanel里面的顺序
        /// </summary>
        public int GetIndex()
        {
            StackPanel panel = ControlTreeHelper.FindParentOfType<StackPanel>(this);
            return panel.Children.IndexOf(this);
        }
    }
}
