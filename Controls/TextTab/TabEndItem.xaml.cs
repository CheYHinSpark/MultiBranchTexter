using MultiBranchTexter.Model;
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

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// TabEndItem.xaml 的交互逻辑
    /// </summary>
    public partial class TabEndItem : UserControl
    {
        private TextNode textNode;
        public TextNode nextNode;

        public TabEndItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 新建TabEndItem
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="answer"></param>
        /// <param name="postNode"></param>
        public TabEndItem(TextNode newNode, string answer, TextNode postNode)
        {
            InitializeComponent();
            textNode = newNode;
            answerTxt.Text = answer;
            nextNode = postNode;
            if (postNode != null)//有可能为null
            { postNameTxt.Text = postNode.Name; }
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (nextNode == null)
            { return; }
            (Application.Current.MainWindow as MainWindow).OpenMBTabItem(nextNode);
        }

        private void changeBtn_Click(object sender, RoutedEventArgs e)
        {
            ControlTreeHelper
                .FindParentOfType<MainWindow>(this)
                .BackToFront(textNode);
        }
    }
}
