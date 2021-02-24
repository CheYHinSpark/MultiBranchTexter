using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static DependencyProperty AllowDoubleEnterProperty =
          DependencyProperty.Register("AllowDoubleEnter", typeof(bool),
              typeof(TextFragmentPresenter), new PropertyMetadata(false));

        public bool AllowDoubleEnter
        {
            get { return (bool)GetValue(AllowDoubleEnterProperty); }
            set { SetValue(AllowDoubleEnterProperty, value); }
        }


        private readonly MBTabItem ownerTab;
        private StackPanel parentPanel;
        private readonly TextFragment fragment;
        public TextFragment Fragment 
        {
            get
            {
                SaveFragment();
                return fragment;
            }
        }

        public int OperationCount { get { return GetOperationLineCount(); } }
        public string ContentText { get { return contentContainer.Text; } }


        #region 构造方法
        public TextFragmentPresenter(MBTabItem owner)
        {
            InitializeComponent();
            fragment = new TextFragment();
            ownerTab = owner;
            Loaded += TextFragmentPresenter_Loaded;
        }

        public TextFragmentPresenter(TextFragment textFragment, MBTabItem owner)
        {
            InitializeComponent();
            fragment = textFragment;
            ownerTab = owner;
            Loaded += TextFragmentPresenter_Loaded;
        }
        #endregion

        #region 事件
        private void TextFragmentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            parentPanel = ControlTreeHelper.FindParentOfType<StackPanel>(this);
            contentContainer.Text = fragment.Content;
            for (int i = 0; i < fragment.Operations.Count; i++)
            { opContainer.Text += fragment.Operations[i] + "\r\n"; }

            opContainer.TextChanged += Operation_TextChanged;
            contentContainer.TextChanged += Content_TextChanged;
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
                    From = 2,
                    To = opContainer.ActualHeight,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
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
                    To = 2,
                    Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
                    FillBehavior = FillBehavior.Stop
                };
                sb.Children.Add(da);
                Storyboard.SetTarget(da, opContainer);
                Storyboard.SetTargetProperty(da, new PropertyPath("Height"));
                sb.Completed += delegate { opContainer.Height = 2; };
                sb.Begin();
            }
        }

        /// <summary>
        /// 片段失去焦点
        /// </summary>
        private void Fragment_LostFocus(object sender, RoutedEventArgs e)
        {
            //完成修改
            SaveFragment();
        }

        //content变化
        private void Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            RaiseChange();

            int i = 0;
            if (contentContainer.IsKeyboardFocusWithin)
            {
                i = contentContainer.SelectionStart;
                if (i < 4)
                { return; }
            }
            else
            { return; }

            if (!AllowDoubleEnter)
            {
                try
                {
                    //如果检测到两个换行符，则会分割文段
                    if (contentContainer.Text[i - 1] == '\n' && contentContainer.Text[i - 3] == '\n')
                    {
                        string oldF = contentContainer.Text.Substring(0, i - 4);
                        string newF = contentContainer.Text[(i)..];
                        TextFragmentPresenter tfp = new TextFragmentPresenter(new TextFragment(newF), ownerTab);
                        parentPanel.Children.Insert(parentPanel.Children.IndexOf(this) + 1, tfp);
                        parentPanel.UpdateLayout();// <--必须有
                        tfp.GetFocus();
                        tfp.contentContainer.Select(0, 0);
                        contentContainer.Text = oldF;
                    }
                }
                catch { }
            }
        }

        private void Operation_TextChanged(object sender, TextChangedEventArgs e)
        {
            RaiseChange();
        }

        private void Content_KeyDown(object sender, KeyEventArgs e)
        {
            //如果在开头按下退格，且本片段没有operation，则合并本段与上段
            if (e.Key == Key.Back && contentContainer.SelectionStart == 0 && OperationCount == 0)
            {
                e.Handled = true;// <--否则会多退格
                int i = parentPanel.Children.IndexOf(this);
                if (i > 0)
                {
                    (parentPanel.Children[i - 1] as TextFragmentPresenter).AppendContent(ContentText);
                    RaiseChange();
                    parentPanel.Children.Remove(this);
                }
            }
            //如果在末尾按下删除，且下一片段没有operation，则合并本段与下一段
            if (e.Key == Key.Delete && contentContainer.SelectionStart == contentContainer.Text.Length)
            {
                e.Handled = true;// <--否则会多退格
                int i = parentPanel.Children.IndexOf(this);
                if (i < parentPanel.Children.Count - 1 && (parentPanel.Children[i + 1] as TextFragmentPresenter).OperationCount == 0)
                {
                    AppendContent((parentPanel.Children[i + 1] as TextFragmentPresenter).ContentText);
                    parentPanel.Children.RemoveAt(i + 1);
                    RaiseChange();
                }
            }
            //如果按下了Ctrl+Enter，则切断fragment
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                int i = contentContainer.SelectionStart;
                string oldF = contentContainer.Text.Substring(0, i);
                string newF = contentContainer.Text[(i)..];
                TextFragmentPresenter tfp = new TextFragmentPresenter(new TextFragment(newF), ownerTab);
                parentPanel.Children.Insert(parentPanel.Children.IndexOf(this) + 1, tfp);
                parentPanel.UpdateLayout();// <--必须有
                tfp.GetFocus();
                tfp.contentContainer.Select(0, 0);
                contentContainer.Text = oldF;
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
        /// 移动光标到最后
        /// </summary>
        public void SelecteLast()
        {
            contentContainer.SelectionStart = contentContainer.Text.Length;
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
            int linecount = 0;
            string newtext = "";
            for (int i = 0; i < opContainer.LineCount;i++)
            {
                string temp = opContainer.GetLineText(i).Replace("\r", "").Replace("\n", "");
                if (temp.Replace(" ","") != "")
                {
                    linecount++;
                    newtext += temp + "\r\n";
                }
            }
            opContainer.Text = newtext;
            return linecount;
        }

        private void SaveFragment()
        {
            GetOperationLineCount();
            fragment.Content = contentContainer.Text;
            fragment.Operations = new List<string>();
            for (int i = 0; i < opContainer.LineCount; i++)
            {
                string str = opContainer.GetLineText(i);
                if (str != "")
                { fragment.Operations.Add(str); }
            }
        }

        //向上级通知改变
        private void RaiseChange()
        {
            ownerTab.ViewModel.IsModified = "*";
            (Application.Current.MainWindow as MainWindow).GetFCC().IsModified = "*";
        }
        #endregion
    }
}
