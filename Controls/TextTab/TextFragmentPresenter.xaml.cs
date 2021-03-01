using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// TextFragmentPresenter.xaml 的交互逻辑
    /// </summary>
    public partial class TextFragmentPresenter : UserControl
    {
        #region 依赖属性
        /// <summary>
        /// 注释文本
        /// </summary>
        public static DependencyProperty CommentTextProperty =
         DependencyProperty.Register("CommentText", //属性名称
             typeof(string), //属性类型
             typeof(TextFragmentPresenter), //该属性所有者，即将该属性注册到那个类上
             new PropertyMetadata("")//属性默认值
             );

        public string CommentText
        {
            get { return (string)GetValue(CommentTextProperty); }
            set { SetValue(CommentTextProperty, value); }
        }

        /// <summary>
        /// 内容文本
        /// </summary>
        public static DependencyProperty ContentTextProperty =
      DependencyProperty.Register("ContentText", //属性名称
          typeof(string), //属性类型
          typeof(TextFragmentPresenter), //该属性所有者，即将该属性注册到那个类上
          new PropertyMetadata("")//属性默认值
          );

        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }
        #endregion

        #region 构造方法
        public TextFragmentPresenter()
        {
            InitializeComponent();
            this.Loaded += TextFragmentPresenter_Loaded;
        }
        #endregion

        #region 事件
        private void TextFragmentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            contentContainer.TextChanged += Content_TextChanged;
            if (CommentText == null || CommentText == "")
            {
                commentContainer.Height = 2;
                showHideOpBtn.IsChecked = false;
            }
            else
            { showHideOpBtn.IsChecked = true; }
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
                commentContainer.Height = double.NaN;
                commentContainer.UpdateLayout();// <--必须，否则没有动画效果
                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation()
                {
                    From = 2,
                    To = commentContainer.ActualHeight,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                    FillBehavior = FillBehavior.Stop
                };
                sb.Children.Add(da);
                Storyboard.SetTarget(da, commentContainer);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Completed += delegate { commentContainer.Height = double.NaN; };
                sb.Begin();
            }
            else
            {
                Storyboard sb = new Storyboard();
                DoubleAnimation da = new DoubleAnimation()
                {
                    From = commentContainer.ActualHeight,
                    To = 2,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                    FillBehavior = FillBehavior.Stop
                };
                sb.Children.Add(da);
                Storyboard.SetTarget(da, commentContainer);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Completed += delegate { commentContainer.Height = 2; };
                sb.Begin();
            }
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

            if (!(ViewModelFactory.Settings.AllowDoubleEnter == true))
            {
                try
                {
                    //如果检测到两个换行符，则会分割文段
                    if (contentContainer.Text[i - 1] == '\n' && contentContainer.Text[i - 3] == '\n')
                    {
                        string oldF = contentContainer.Text.Substring(0, i - 4);
                        string newF = contentContainer.Text[(i)..];
                        i = (int)Tag;
                        ViewModelFactory.Main.WorkingTab.BreakFragment(i, oldF, newF);
                    }
                }
                catch { }
            }
        }

        private void Content_KeyDown(object sender, KeyEventArgs e)
        {
            //如果在开头按下退格，且本片段没有operation，则合并本段与上段
            if (e.Key == Key.Back
                && contentContainer.SelectionStart == 0
                && contentContainer.SelectionLength == 0
                && (CommentText == null || CommentText == ""))
            {
                e.Handled = true;// <--否则会多退格
                int i = (int)Tag;
                if (i > 0)
                {
                    ViewModelFactory.Main.WorkingTab.GlueFragment(i - 1);
                }
            }
            //如果在末尾按下删除，且下一片段没有operation，则合并本段与下一段
            if (e.Key == Key.Delete 
                && contentContainer.SelectionStart == ContentText.Length
                && contentContainer.SelectionLength == 0)
            {
                e.Handled = true;// <--否则会多退格
                int i = (int)Tag;
                if (i < ViewModelFactory.Main.WorkingViewModel.TextFragments.Count - 1 )
                {
                    ViewModelFactory.Main.WorkingTab.GlueFragment(i);
                }
            }
            //如果按下了Ctrl+Enter，则切断fragment
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                int i = contentContainer.SelectionStart;
                string oldF = contentContainer.Text.Substring(0, i);
                string newF = contentContainer.Text[(i)..];
                i = (int)Tag;
                ViewModelFactory.Main.WorkingTab.BreakFragment(i, oldF, newF);
            }
        }


        #region tabitemviewmodel逻辑
        private void Container_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel.FocusInfo = null;
        }

        private void CommentContainer_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel
                .SetFocusSelectInfo((int)Tag, false,
                commentContainer.SelectionStart,
                commentContainer.SelectionLength);
        }

        private void ContentContainer_GotFocus(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel
                .SetFocusSelectInfo((int)Tag, true,
                contentContainer.SelectionStart,
                contentContainer.SelectionLength);
        }

        private void CommentContainer_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel.SelectionLength = commentContainer.SelectionLength;
            ViewModelFactory.Main.WorkingViewModel.SelectionStart = commentContainer.SelectionStart;
        }

        private void ContentContainer_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Main.WorkingViewModel.SelectionLength = contentContainer.SelectionLength;
            ViewModelFactory.Main.WorkingViewModel.SelectionStart = contentContainer.SelectionStart;
        }
        #endregion

        #endregion

        #region 方法
        public void SetFocus(int SelectS, bool isContent = true, int SelectL = 0)
        {
            if (isContent)
            {
                Keyboard.Focus(contentContainer);
                contentContainer.Focus();
                if (SelectS >= 0 && SelectS <= ContentText.Length)
                {  contentContainer.SelectionStart = SelectS; }
                else { contentContainer.SelectionStart = ContentText.Length; }
                contentContainer.SelectionLength = SelectL;
            }
            else
            {
                Keyboard.Focus(commentContainer);
                commentContainer.Focus();
                if (SelectS >= 0 && SelectS <= CommentText.Length)
                { commentContainer.SelectionStart = SelectS; }
                else { commentContainer.SelectionStart = CommentText.Length; }
                commentContainer.SelectionLength = SelectL;
            }
        }

        //对operation做行计算，并消除空行
        [Obsolete]
        private int GetOperationLineCount()
        {
            int linecount = 0;
            string newtext = "";
            for (int i = 0; i < commentContainer.LineCount;i++)
            {
                string temp = commentContainer.GetLineText(i).Replace("\r", "").Replace("\n", "");
                if (temp.Replace(" ","") != "")
                {
                    linecount++;
                    newtext += temp + "\r\n";
                }
            }
            if (linecount > 0)//去掉最后的换行
            { newtext = newtext[..^2]; }
            commentContainer.Text = newtext;
            return linecount;
        }
        #endregion
    }
}
