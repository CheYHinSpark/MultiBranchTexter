using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    [Obsolete]
    public partial class TextNodePresenter : UserControl
    {
        private TextNode textNode;
        public TextNode TextNode { get { return textNode; } }

        public TextNodePresenter()
        {
            InitializeComponent();
            LoadNode(new TextNode());
        }

        public TextNodePresenter(TextNode node)
        {
            InitializeComponent();
            LoadNode(node);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;// <--必须有
            (fragmentContainer.Children[^1] as TextFragmentPresenter).GetFocus();
        }

        /// <summary>
        /// 载入节点，根据其内容和后继生成相应的东西
        /// </summary>
        public void LoadNode(TextNode node)
        {
            textNode = node;
            fragmentContainer.Children.Clear();

            for (int i = 0; i < textNode.Fragments.Count; i++)
            { fragmentContainer.Children.Add(new TextFragmentPresenter(textNode.Fragments[i])); }

            if (fragmentContainer.Children.Count == 0)//至少要有一个
            { fragmentContainer.Children.Add(new TextFragmentPresenter()); }

            //TODO 载入后继信息

        }


        /// <summary>
        /// 保存节点
        /// </summary>
        public void SaveNode()
        {
            List<TextFragment> newFragments = new List<TextFragment>();
            for (int i =0;i<fragmentContainer.Children.Count;i++)
            {
                newFragments.Add((fragmentContainer.Children[i] as TextFragmentPresenter).Fragment);
            }
            textNode.Fragments.Clear();
            textNode.Fragments = newFragments;
        }
    }
}
