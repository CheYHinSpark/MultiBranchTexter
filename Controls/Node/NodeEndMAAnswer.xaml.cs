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
        private string answer = "";
        private readonly string nextNodeName = "";

        /// <summary>
        /// 获取当前的回答文本
        /// </summary>
        public string Answer
        { 
            get 
            {
                if (answerTxt == null)
                { return answer; }
                return answerTxt.Text; 
            }
        }

        public NodeEndMAAnswer()
        {
            InitializeComponent();
        }
        public NodeEndMAAnswer(string newAnswer, string postNodeName)
        {
            InitializeComponent();
            answer = newAnswer;
            nextNodeName = postNodeName;
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

            answerTxt.Text = answer;
        }

        //双击，准备调整后继节点
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
            if (answerTxt.Text == answer)//这说明没有修改
            { return; }
            if (answerTxt.Text == "")//禁止为空
            {
                answerTxt.Text = answer;
                MessageBox.Show("回答不能为空，已还原");
                return;
            }
            //取得节点的后继条件
            EndCondition ec = FatherTextNode.endCondition;
            if (ec.Answers.ContainsKey(Answer))
            { 
                answerTxt.Text = answer;
                MessageBox.Show("回答出现重复，已还原");
            }
            else
            {
                FatherNode.ChangeAnswer(answer, answerTxt.Text);
                //ec.Answers.Remove(answer);
                //没有重复，完成修改
                answer = answerTxt.Text;
                //ec.Answers.Add(answer, nextNodeName);
                //通知标签页改变
                ControlTreeHelper.FindParentOfType<MainWindow>(FatherNode).ReLoadTab(FatherTextNode);
            }
            answerTxt.SelectionStart = 0;
        }

        private void MaCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //如果FCC处于等待选择新的后继节点状态，需要取消
            FlowChartContainer fcc = ControlTreeHelper.FindParentOfType<FlowChartContainer>(FatherNode);
            fcc.PostNodeChoosed(null);
            //断开连接
            ConnectingLine line = null;
            for (int i = 0;i < FatherNode.postLines.Count;i++)
            {
                if (FatherNode.postLines[i].BeginNode == this)
                {
                    line = FatherNode.postLines[i];
                }
            }
            //可能没有连线
            if (line != null)
            {
                NodeButton.UnLink(this, line.EndNode);
                line.Delete();
            }
            //如果没有连线，也要删除相应的key
            FatherNode.answerToNodes.Remove(answer);
            FatherTextNode.endCondition.Answers.Remove(answer);
            //通知标签页改变
            ControlTreeHelper.FindParentOfType<MainWindow>(FatherNode).ReLoadTab(FatherTextNode);
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
