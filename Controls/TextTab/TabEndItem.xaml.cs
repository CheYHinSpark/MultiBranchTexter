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
        public string nextNodeName;

        public TabEndItem()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 新建TabEndItem
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="postNode"></param>
        public TabEndItem(string answer, string postName)
        {
            InitializeComponent();
            answerTxt.Text = answer;
            nextNodeName = postName;
            if (postName != "")//有可能为null
            { postNameTxt.Text = postName; }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (nextNodeName == "")
            { return; }
            (Application.Current.MainWindow as MainWindow).OpenMBTabItem(nextNodeName);
        }
    }
}
