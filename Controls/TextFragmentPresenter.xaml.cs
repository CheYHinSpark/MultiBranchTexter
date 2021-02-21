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
using System.Windows.Media.Animation;
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
        private readonly TextFragment fragment;
        public TextFragment Fragment { get { return fragment; } }

        public int OperationCount { get { return GetOperationLineCount(); } }
        public string ContentText { get { return contentContainer.Text; } }


        #region 构造方法
        public TextFragmentPresenter()
        {
            InitializeComponent();
            fragment = new TextFragment();
            Loaded += TextFragmentPresenter_Loaded;
        }

        public TextFragmentPresenter(TextFragment textFragment)
        {
            InitializeComponent();
            Loaded += TextFragmentPresenter_Loaded;

            fragment = textFragment;
            contentContainer.Text = fragment.Content;
        }
        #endregion

        #region 事件
        private void TextFragmentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            parent = ControlTreeHelper.FindParentOfType<StackPanel>(this);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            showHideOpBtn.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            showHideOpBtn.Visibility = Visibility.Hidden;
        }

        private void ShowHideOpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (showHideOpBtn.IsChecked == true)
            {
                opContainer.Height = double.NaN;
                opContainer.UpdateLayout();// <--必须，否则没有动画效果
                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation()
                {
                    From = 0,
                    To = opContainer.ActualHeight,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)),
                    FillBehavior = FillBehavior.Stop
                };
                sb.Children.Add(da);
                Storyboard.SetTarget(da, opContainer);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Completed += delegate { opContainer.Height = double.NaN; };
                sb.Begin();
            }
            else
            {
                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation()
                {
                    From = opContainer.ActualHeight,
                    To = 0,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300)),
                    FillBehavior = FillBehavior.Stop
                };
                sb.Children.Add(da);
                Storyboard.SetTarget(da, opContainer);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Completed += delegate { opContainer.Height = 0; };
                sb.Begin();
            }
        }

        /// <summary>
        /// 片段失去焦点
        /// </summary>
        private void Fragment_LostFocus(object sender, RoutedEventArgs e)
        {
            //完成修改
            GetOperationLineCount();
            SaveFragment();
        }

        //content变化
        private void Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            int i = 0;
            if (contentContainer.IsKeyboardFocusWithin)
            {
                i = contentContainer.SelectionStart;
                if (i < 4)
                { return; }
            }
            else
            { return; }

            try
            {
                //如果检测到两个换行符，则会分割文段
                if (contentContainer.Text[i - 1] == '\n' && contentContainer.Text[i - 3] == '\n')
                {
                    string oldF = contentContainer.Text.Substring(0, i - 4);
                    string newF = contentContainer.Text[(i)..];
                    TextFragmentPresenter tfp = new TextFragmentPresenter(new TextFragment(newF));
                    parent.Children.Insert(parent.Children.IndexOf(this) + 1, tfp);
                    parent.UpdateLayout();// <--必须有
                    tfp.GetFocus();
                    tfp.contentContainer.Select(0, 0);
                    contentContainer.Text = oldF;
                }
            }
            catch { }
        }

        private void Content_KeyDown(object sender, KeyEventArgs e)
        {
            //如果在开头按下退格，且本片段没有operation，则合并本段与上段
            if (e.Key == Key.Back && contentContainer.SelectionStart == 0 && OperationCount == 0)
            {
                e.Handled = true;// <--否则会多退格
                int i = parent.Children.IndexOf(this);
                if (i > 0)
                {
                    (parent.Children[i - 1] as TextFragmentPresenter).AppendContent(ContentText);
                    parent.Children.Remove(this);
                }
            }
            //如果在末尾按下删除，且下一片段没有operation，则合并本段与下一段
            if (e.Key == Key.Delete && contentContainer.SelectionStart == contentContainer.Text.Length)
            {
                e.Handled = true;// <--否则会多退格
                int i = parent.Children.IndexOf(this);
                if (i < parent.Children.Count - 1 && (parent.Children[i + 1] as TextFragmentPresenter).OperationCount == 0)
                {
                    AppendContent((parent.Children[i + 1] as TextFragmentPresenter).ContentText);
                    parent.Children.RemoveAt(i + 1);
                }
            }
        }
        #endregion

        #region 方法
        public void GetFocus()
        {
            Keyboard.Focus(contentContainer);
            contentContainer.Focus();
        }

        /// <summary>
        /// 直接在后面加入一段内容，光标会回到原来内容的末尾
        /// </summary>
        public void AppendContent(string content)
        {
            int i = contentContainer.Text.Length;
            contentContainer.Text += content;

            GetFocus();
            contentContainer.SelectionStart = i;
        }

        //对operation做行计算，并消除空行
        private int GetOperationLineCount()
        {
            int linecount = opContainer.LineCount;
            bool b = true;
            while (b)
            {
                opContainer.Text = opContainer.Text.Replace("\n\r\n", "\n");//去掉空行
                if (linecount == opContainer.LineCount)
                { b = false; }
                else
                { linecount = opContainer.LineCount; }
            }

            if (opContainer.GetLineText(linecount - 1) == "")
            {
                linecount--;
            }
            return linecount;
        }

        private void SaveFragment()
        {
            fragment.Content = contentContainer.Text;
            fragment.Operations = new List<string>();
            for (int i = 0; i < opContainer.LineCount; i++)
            {
                string str = opContainer.GetLineText(i);
                if (str != "")
                { fragment.Operations.Add(str); }
            }
        }
        #endregion
    }
}
