using MultiBranchTexter.Model;
using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// MBTabItem.xaml 的交互逻辑
    /// </summary>
    public partial class MBTabItem : TabItem
    {
        public MBTabItem(TextNode node)
        {
            InitializeComponent();
            TextNode = node;
            Header = node.Name;
            tabEnd.SetTabEnd(node);
            _viewModel = DataContext as TabItemViewModel;
            LoadNode(node);
            _viewModel.IsModified = "";
        }

        #region 成员变量
        /// <summary> 相应的textNode </summary>
        public TextNode TextNode { get; set; }

        private readonly TabItemViewModel _viewModel;

        public TabItemViewModel ViewModel { get { return _viewModel; } }

        //拖动
        private bool _isDragging;

        //触摸
        private Point _touchPoint;
        #endregion

        #region 事件
        /// <summary> Loaded </summary>
        private void TabItem_Loaded(object sender, RoutedEventArgs e)
        {
            // 查找控件模板
            if (App.Current.Resources["MBTabItemTemplate"] is ControlTemplate tabItemTemplate)
            {
                (tabItemTemplate.FindName("CloseBtn", this) as Button).Click += CloseBtn_Click;
            }

            ViewModel.ClearUndoStack();

            Binding binding = new Binding
            {
                Source = ViewModelFactory.Settings,
                Path = new PropertyPath("SideWidth")
            };

            lCol.SetBinding(ColumnDefinition.WidthProperty, binding);
            rCol.SetBinding(ColumnDefinition.WidthProperty, binding);

            this.Width = 0;//否则动画不流畅
            ViewModelFactory.Main.ReSizeTabs();
        }

        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        { Close(false); }


        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;// <--必须有
            int i = _viewModel.TextFragments.Count - 1;
            ContentPresenter cp = fragmentContainer.ItemContainerGenerator
                          .ContainerFromIndex(i) as ContentPresenter;
            DataTemplate MyDataTemplate = cp.ContentTemplate;

            if (MyDataTemplate.FindName("TFP", cp) is TextFragmentPresenter tfp)
            {
                tfp.SetFocus(-1);
            }
        }

        #endregion

        #region Node相关方法

        /// <summary> 重置Tab </summary>
        public void ReLoadTab()
        {
            Header = TextNode.Name;
            tabEnd.LoadTabEnd();
        }

        /// <summary> 关闭自身 </summary>
        /// <param name="force"> 是否强制关闭 </param>
        public void Close(bool force)
        {
            //检查保存
            if (_viewModel.IsModified == "*" && !force)
            {
                MessageBoxResult result = MessageBox.Show(
                    Application.Current.MainWindow,
                    "文本尚未保存，是否保存？",
                    "警告",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Cancel);
                if (result == MessageBoxResult.Cancel)
                { return; }
                else if (result == MessageBoxResult.Yes)
                { Save(); }
            }
            ////移除自身
            ToWidth(0, true);
        }

        public void Save()
        {
            if (_viewModel.IsModified == "")
            { return; }
            List<TextFragment> newFragments = new List<TextFragment>();
          
            for (int i = 0; i < _viewModel.TextFragments.Count; i++)
            {
                newFragments.Add(_viewModel.TextFragments[i]);
            }

            TextNode.Fragments = newFragments;
            _viewModel.IsModified = "";
            ViewModelFactory.Main.RaiseHint("节点 " + TextNode.Name + " 保存成功");
        }

        /// <summary>
        /// 载入节点，根据其内容和后继生成相应的东西
        /// </summary>
        public void LoadNode(TextNode node)
        {
            TextNode = node;
          
            for (int i = 0; i < TextNode.Fragments.Count; i++)
            { _viewModel.TextFragments.Add(TextNode.Fragments[i]); }

            if (_viewModel.TextFragments.Count == 0)//至少要有一个
            { _viewModel.TextFragments.Add(new TextFragment()); }

            _viewModel.CountCharWord(true);
        }


        /// <summary>
        /// 将i处节点与后面一个节点合并
        /// </summary>
        public void GlueFragment(int i)
        {
            int s = _viewModel.TextFragments[i].Content.Length;

            _viewModel.TextFragments[i].Content += _viewModel.TextFragments[i + 1].Content;
            _viewModel.TextFragments.RemoveAt(i + 1);

            ContentPresenter cp = fragmentContainer.ItemContainerGenerator
                      .ContainerFromIndex(i) as ContentPresenter;
            DataTemplate MyDataTemplate = cp.ContentTemplate;

            if (MyDataTemplate.FindName("TFP", cp) is TextFragmentPresenter tfp)
            {
                tfp.SetFocus(s);
            }
        }

        /// <summary>
        /// 切开old index处的片段，将oldContent赋予old,新的给新的
        /// </summary>
        public async void BreakFragment(int oldIndex, string oldContent, string newContent)
        {
            _viewModel.TextFragments.Insert(oldIndex + 1, new TextFragment(newContent));
            _viewModel.TextFragments[oldIndex].Content = oldContent;

            await Task.Delay(10);//等新的东西准备好
            ContentPresenter cp = fragmentContainer.ItemContainerGenerator
                      .ContainerFromIndex(oldIndex + 1) as ContentPresenter;
            DataTemplate MyDataTemplate = cp.ContentTemplate;

            if (MyDataTemplate.FindName("TFP", cp) is TextFragmentPresenter tfp)
            {
                tfp.SetFocus(0);
            }
        }

        /// <summary> 用动画调整到指定宽度 </summary>
        public void ToWidth(double width, bool close = false)
        {
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
            SplineDoubleKeyFrame sdkf = new SplineDoubleKeyFrame(width,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200)),
                new KeySpline(0.5, 1.0, 0.5, 1.0));
            da.KeyFrames.Add(sdkf);

            if (close)
            {
                da.Completed += delegate
                {
                    //移除自身
                    ViewModelFactory.Main.WorkTabs.Remove(this);
                    ViewModelFactory.Main.ReSizeTabs();
                };
            }
             BeginAnimation(WidthProperty, da);
            
        }
        #endregion

        #region 拖拽标签页相关override方法
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (VisualTreeHelper.HitTest(this, e.GetPosition(this)) == null) return;

            if ( !_isDragging)
            {
                _isDragging = true;
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ( _isDragging)
            {
                var p = e.GetPosition(this);
       
                if (p.X > this.ActualWidth)
                {
                    int i = ViewModelFactory.Main.WorkTabs.IndexOf(this);
                    if (i < ViewModelFactory.Main.WorkTabs.Count - 1)
                    {
                        ViewModelFactory.Main.WorkTabs.Move(i + 1, i);
                    }
                }
                if (p.X < 0)
                {
                    int i = ViewModelFactory.Main.WorkTabs.IndexOf(this);
                    if (i > 0)
                    {
                        ViewModelFactory.Main.WorkTabs.Move(i - 1, i);
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            ReleaseMouseCapture();

            _isDragging = false;
        }
        #endregion

        #region 触摸屏支持
        private void ScrollViewer_TouchDown(object sender, TouchEventArgs e)
        {
            _touchPoint = e.GetTouchPoint(this).Position;
            fragmentContainer.IsHitTestVisible = false;
        }

        private void ScrollViewer_TouchMove(object sender, TouchEventArgs e)
        {
            Point point = e.GetTouchPoint(this).Position;
            ScrollViewer sv = sender as ScrollViewer;
            sv.ScrollToVerticalOffset(sv.VerticalOffset - point.Y + _touchPoint.Y);
            sv.ScrollToHorizontalOffset(sv.HorizontalOffset - point.X + _touchPoint.X);
            _touchPoint = point;
        }

        private void ScrollViewer_TouchLeaveUp(object sender, TouchEventArgs e)
        {
            fragmentContainer.IsHitTestVisible = true;
        }
        #endregion
    }
}
