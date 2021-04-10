using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiBranchTexter.Model;
using MultiBranchTexter.Resources;
using MultiBranchTexter.ViewModel;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 多选后继节点的单个答案节点
    /// </summary>
    public partial class NodeEndMAAnswer : NodeBase
    {
        //private Button maCloseBtn;
        //private TextBox hintTxt;
        //private TextBox answerTxt;
        private string answer = "";

        /// <summary>
        /// 获取当前的回答文本
        /// </summary>
        public string Answer
        { 
            get 
            {
                if (answerTxt.Text == null || answerTxt.Text == "")
                { return answer; }
                return answerTxt.Text; 
            }
        }

        public NodeEndMAAnswer()
        {
            InitializeComponent();
        }

        public NodeEndMAAnswer(string newAnswer)
        {
            InitializeComponent();
            answer = newAnswer;
        }

        #region 事件
        //加载完成
        private void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            //maCloseBtn = GetTemplateChild("maCloseBtn") as Button;
            //hintTxt = GetTemplateChild("hintTxt") as TextBox;
            //answerTxt = GetTemplateChild("answerTxt") as TextBox;

            maCloseBtn.Click += MaCloseBtn_Click;
            hintTxt.LostFocus += HintTxt_LostFocus;
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
                if (ViewModelFactory.FCC.IsWaiting)
                { return; }
                //进入选择模式
                ViewModelFactory.FCC.WaitClick(this);
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
                MessageBox.Show(LanguageManager.Instance["Msg_AEmpty"]);
                return;
            }
            //取得节点的后继条件
            EndCondition ec = FatherTextNode.EndCondition;
            for (int i = 0; i < ec.Answers.Count; i++)
            {
                if (ec.Answers[i].Item1 == Answer)
                {
                    answerTxt.Text = answer;
                    MessageBox.Show(LanguageManager.Instance["Msg_ARepeated"]);
                    return;
                } 
            }

            FatherNode.ChangeAnswer(answer, answerTxt.Text);
            //没有重复，完成修改
            answer = answerTxt.Text;
            //通知标签页改变
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
            answerTxt.SelectionStart = 0;
            FatherNode.UpdateLines();
        }

        // 修改hint，即item3
        private void HintTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            FatherNode.ChangeHint(answer, hintTxt.Text);
            //通知标签页改变
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
            hintTxt.SelectionStart = 0;
            FatherNode.UpdateLines();
        }

        private void MaCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            //如果FCC处于等待选择新的后继节点状态，需要取消
            ViewModelFactory.FCC.PostNodeChoosed(null);
            //断开连接
            ConnectingLine line = null;
            for (int i = 0;i < FatherNode.PostLines.Count;i++)
            {
                if (FatherNode.PostLines[i].BeginNode == this)
                {
                    line = FatherNode.PostLines[i];
                }
            }
            //可能没有连线
            if (line != null)
            {
                NodeButton.UnLink(this, false);
                line.Delete();
            }
            //如果没有连线，也要删除相应的key
            FatherNode.AnswerToNodes.Remove(answer);
            for (int i = 0; i < FatherTextNode.EndCondition.Answers.Count; i++)
            {
                if (answer == FatherTextNode.EndCondition.Answers[i].Item1)
                { 
                    FatherTextNode.EndCondition.Answers.RemoveAt(i);
                    break;
                }
            }
            //通知标签页改变
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
            //移除自身
            ControlTreeHelper.FindParentOfType<NodeEndMA>(this).RemoveAnswer(this);
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
