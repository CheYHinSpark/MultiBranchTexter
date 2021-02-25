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
            foreach (string answer in maEnd.Answers.Keys)
            {
                NodeEndMAAnswer nodeEnd = new NodeEndMAAnswer(answer);
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
            FatherTextNode.endCondition.Question = titleBox.Text;
            titleBox.SelectionStart = 0;
            // 还要通知窗口改变相应的标签页
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
        }

        //点击添加按钮
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string,string> atns = FatherTextNode.endCondition.Answers;
            //创造一个不重名的回答
            string newAnswer = "新回答";
            int i = 1;
            while (atns.ContainsKey(newAnswer + i.ToString()))
            { i++; }
            newAnswer += i.ToString();
            //添加键
            atns.Add(newAnswer,"");
            FatherNode.answerToNodes.Add(newAnswer, null);
            NodeEndMAAnswer nodeEnd = new NodeEndMAAnswer(newAnswer)
            { FatherNode = this.FatherNode };
            answerContainer.Children.Add(nodeEnd);
            // 还要通知窗口改变相应的标签页
            ViewModelFactory.Main.ReLoadTab(FatherTextNode);
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
            }
        }
        #endregion
    }
}
