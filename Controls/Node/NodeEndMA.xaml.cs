using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// NodeEndMA.xaml 的交互逻辑
    /// </summary>
    public partial class NodeEndMA : NodeBase
    {
        public NodeEndMA()
        {
            InitializeComponent();
        }

        public NodeEndMA(EndCondition maEnd)
        {
            InitializeComponent();
            titleBox.Text = maEnd.Question;
            isQuestionBtn.IsChecked = maEnd.IsQuestion;
            for (int i =0;i<maEnd.Answers.Count;i++)
            {
                NodeEndMAAnswer nodeEnd = new NodeEndMAAnswer(maEnd.Answers[i].Item1);
                answerContainer.Children.Add(nodeEnd);
            }
        }

        #region 事件
        //加载完成
        private void NodeBase_Loaded(object sender, RoutedEventArgs e)
        {
            //设置显示顺序为2，以显示在connectingline上面
            Panel.SetZIndex(this, 2);
        }

        //双击标题，可以改标题
        private void TitleBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            titleBox.Focusable = true;
        }

        //标题失去焦点，恢复为不可聚焦并完成标题修改
        private void TitleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            titleBox.Focusable = false;
            // 完成问题修改
            FatherTextNode.EndCondition.Question = titleBox.Text;
            titleBox.SelectionStart = 0;
            // 还要通知窗口改变相应的标签页
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
            FatherNode.UpdateLines();
        }

        //点击添加按钮
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            List<(string,string)> atns = FatherTextNode.EndCondition.Answers;
            //创造一个不重名的回答
            string newAnswer = "新回答";
            int i = 0;
            bool repeated = true;
            while (repeated)
            {
                i++;
                bool b = true;
                for (int j = 0; j < atns.Count; j++)
                { b &= atns[j].Item1 != newAnswer + i.ToString(); }
                //全都不重复b才会为真
                repeated = !b;
            }
            newAnswer += i.ToString();
            //添加键
            atns.Add((newAnswer,""));
            FatherNode.answerToNodes.Add(newAnswer, null);
            NodeEndMAAnswer nodeEnd = new NodeEndMAAnswer(newAnswer)
            { FatherNode = this.FatherNode };
            answerContainer.Children.Add(nodeEnd);
            // 还要通知窗口改变相应的标签页
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
        }

        //变更是否是询问
        private void IsQuestionBtn_Click(object sender, RoutedEventArgs e)
        {
            FatherNode.textNode.EndCondition.IsQuestion = isQuestionBtn.IsChecked == true;
            ViewModelFactory.Main.IsModified = true;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置真正的father
        /// </summary>
        public void SetFather(NodeButton father)
        {
            this.FatherNode = father;
            foreach (UserControl control in answerContainer.Children)
            {
                (control as NodeEndMAAnswer).FatherNode = father;
                father.answerToNodes.Add((control as NodeEndMAAnswer).Answer, null);
            }
        }
        #endregion
    }
}
