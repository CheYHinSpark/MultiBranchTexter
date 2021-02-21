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
    /// TextFragmentPresenter.xaml 的交互逻辑
    /// </summary>
    public partial class TextFragmentPresenter : UserControl
    {
        private StackPanel parent;
        private TextFragment fragment;
        public TextFragment Fragment { get { return fragment; } }
        public TextFragmentPresenter()
        {
            InitializeComponent();
            Loaded += TextFragmentPresenter_Loaded;
        }



        public TextFragmentPresenter(TextFragment textFragment)
        {
            InitializeComponent();
            Loaded += TextFragmentPresenter_Loaded;

            fragment = textFragment;
        }


        private void TextFragmentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            parent = ControlTreeHelper.FindParentOfType<StackPanel>(this);
        }

        private void addOpBtn_Click(object sender, RoutedEventArgs e)
        {
            opContainer.Children.Add(new TextBox());
        }

        internal void GetFocus()
        {
            Keyboard.Focus(contentContainer);
        }

        /// <summary>
        /// 片段失去焦点，如果此时content和operation均为空，且这不是唯一的文本片段则会删除自身
        /// </summary>
        private void fragment_LostFocus(object sender, RoutedEventArgs e)
        {
            if (contentContainer.Text == "" &&
                opContainer.Children.Count == 0 &&
                parent.Children.Count > 1)
            {
                parent.Children.Remove(this);
                return;
            }
            //完成修改
            fragment.Content = contentContainer.Text;
        }

        private void addFragmentBtn_Click(object sender, RoutedEventArgs e)
        {
            parent.Children.Insert(parent.Children.IndexOf(this) + 1, new TextFragmentPresenter(new TextFragment()));
        }

        private void closeFragmentBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO警告
            parent.Children.Remove(this);
        }
    }
}
