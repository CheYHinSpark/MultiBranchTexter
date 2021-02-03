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
            answerTxt.TextChanged += AnswerTxt_TextChanged;
            answerTxt.LostFocus += AnswerTxt_LostFocus;

            answerTxt.Text = answerToNode.Answer;
        }

        private void AnswerTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void MaCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO:关闭改节点
            //TODO:断开连接
        }

        private void AnswerTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        #endregion

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
    }
}
