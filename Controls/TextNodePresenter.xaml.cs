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
    public partial class TextNodePresenter : UserControl
    {
        private TextNode textNode;

        public TextNodePresenter()
        {
            InitializeComponent();
        }

        public TextNodePresenter(TextNode node)
        {
            InitializeComponent();
            textNode = node;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;// <--必须有
            (fragmentContainer.Children[^1] as TextFragmentPresenter).GetFocus();
        }

        private void addFragmentBtn_Click(object sender, RoutedEventArgs e)
        {
            fragmentContainer.Children.Add(new TextFragmentPresenter(new TextFragment()));
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
