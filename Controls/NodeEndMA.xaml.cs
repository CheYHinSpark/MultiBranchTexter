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
    /// NodeEndMA.xaml 的交互逻辑
    /// </summary>
    public partial class NodeEndMA : NodeBase
    {
        public NodeEndMA()
        {
            InitializeComponent();
        }
        public NodeEndMA(MultiAnswerCondition mac)
        {
            InitializeComponent();
            titleBox.Text = mac.Question;
            foreach (AnswerToNode atn in mac.AnswerToNodes)
            {
                NodeEndMAAnswer nodeEnd = new NodeEndMAAnswer(atn);
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
        private void titleBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            titleBox.Focusable = true;
        }

        //标题失去焦点，恢复为不可聚焦并完成标题修改
        private void titleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            titleBox.Focusable = false;
            // 完成问题修改
            fatherNode.textNode.endCondition.Question = titleBox.Text;
            titleBox.SelectionStart = 0;
            // TODO 还要通知窗口改变相应的标签页
        }

        //点击添加按钮
        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerToNode atn = new AnswerToNode();
            List<AnswerToNode> atns = (fatherNode.textNode.endCondition as MultiAnswerCondition).AnswerToNodes;
            //创造一个不重名的
            string newAnswer = "新回答";
            int i = 1;
            bool repeated = true;
            while (repeated)
            {
                repeated = false;
                for (int j = 0; j < atns.Count; j++)
                {
                    if (atns[j].Answer == newAnswer + i.ToString())
                    {
                        repeated = true;
                        i++;
                        break;
                    }
                }
            }
            atn.Answer = newAnswer + i.ToString();
            atns.Add(atn);
            NodeEndMAAnswer nodeEnd = new NodeEndMAAnswer(atn);
            nodeEnd.fatherNode = fatherNode;
            answerContainer.Children.Add(nodeEnd);
        }
        #endregion

        #region 方法
        /// <summary>
        /// 设置真正的father
        /// </summary>
        public new void SetFather(NodeButton father)
        {
            this.fatherNode = father;
            foreach (UserControl control in answerContainer.Children)
            {
                (control as NodeEndMAAnswer).fatherNode = father;
            }
        }

        /// <summary>
        /// 输入一个回答，检查回答是否有重复，即数量在2即以上
        /// </summary>
        public bool CheckRepeatedAnswer(string newAnswer)
        {
            int n = 0;
            foreach (UserControl control in answerContainer.Children)
            {
                if ((control as NodeEndMAAnswer).Answer == newAnswer)
                { n++; }
            }
            return n > 1;
        }
        #endregion
    }
}
