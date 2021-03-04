using MultiBranchTexter.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.View
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
            ViewModelFactory.Main.OpenMBTabItem(ViewModelFactory.FCC.GetNodeByName(nextNodeName));
        }
    }
}
