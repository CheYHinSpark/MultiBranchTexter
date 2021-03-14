﻿using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 流程图容器
    /// </summary>
    public partial class FlowChartContainer : UserControl
    {
        //与拖拽、移动有关的变量
        private Point _oldclickPoint;
        private Point _clickPoint;

        private readonly FCCViewModel _viewModel;

        public FlowChartContainer()
        {
            InitializeComponent();
            ViewModelFactory.SetViewModel(typeof(FCCViewModel), this.DataContext as FCCViewModel);
            _viewModel = DataContext as FCCViewModel;
            _viewModel.Container = container;
            _viewModel.ScrollViewer = scrollViewer;
        }

        #region 事件
        //滚轮事件
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // 按住Ctrl，开始放大缩小
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                container.ScaleRatio += 0.1 * Math.Sign(e.Delta);
                container.ScaleRatio = container.ScaleRatio < 0.1 ? 0.1 : container.ScaleRatio;
            }
        }

        //鼠标移动
        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Grid)
            {
                if (Keyboard.Modifiers == ModifierKeys.None && e.LeftButton == MouseButtonState.Pressed && this.Cursor == Cursors.Hand)
                {
                    double x, y;
                    _oldclickPoint = _clickPoint;
                    _clickPoint = e.GetPosition((ScrollViewer)sender);

                    x = _oldclickPoint.X - _clickPoint.X;
                    y = _oldclickPoint.Y - _clickPoint.Y;

                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + x);
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + y);
                }
                else if (Keyboard.Modifiers == ModifierKeys.Alt && e.LeftButton == MouseButtonState.Pressed)
                {
                    double x, y;
                    _oldclickPoint = _clickPoint;
                    _clickPoint = e.GetPosition((ScrollViewer)sender);

                    x = _clickPoint.X - _oldclickPoint.X;
                    y = _clickPoint.Y - _oldclickPoint.Y;

                    for (int i = 0; i < _viewModel.SelectedNodes.Count; i++)
                    {
                        _viewModel.SelectedNodes[i].Move(x / container.ScaleRatio, y / container.ScaleRatio);
                    }

                    ViewModelFactory.Main.IsModified = true;
                }
                else { this.Cursor = Cursors.Arrow; }
            }
            if (selectBorder.Visibility == Visibility.Visible)
            {
                Point p = e.GetPosition((ScrollViewer)sender);
                selectBorder.SetEndPt(p.X, p.Y);
            }
        }

        //点击scrollViewer
        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //更新点击点
            _clickPoint = e.GetPosition((ScrollViewer)sender);
            _oldclickPoint = _clickPoint;
            scrollViewer.StopInertia();
            //表示点击到了空白部分
            if (e.OriginalSource is Grid)
            {
                //右键点击，取消选择后继
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    if (_viewModel.IsWaiting)
                    {
                        _viewModel.PostNodeChoosed(null);
                    }
                    return;
                }
                //非右键点击
                if (Keyboard.Modifiers == ModifierKeys.Control)//同时按下Ctrl
                {
                    selectBorder.Visibility = Visibility.Visible;
                    selectBorder.StartPt = _clickPoint;
                    selectBorder.Width = 0;
                    selectBorder.Height = 0;
                }
                else if (Keyboard.Modifiers != ModifierKeys.Alt)
                    //没有同时按下了Alt或者Ctrl
                {
                    //取消选择
                    _viewModel.ClearSelection();
                    //如果没有打开搜索框，取消搜索
                    if (_viewModel.SearchBoxVisibility == Visibility.Hidden)
                    { _viewModel.ClearSearch(); }
                    //准备拖拽
                    this.Cursor = Cursors.Hand;
                }
            }
        }

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //如果是完成多选
            if (selectBorder.Visibility == Visibility.Visible)
            {
                //需要得到selectBd关于canvas的坐标
                //获得了canvas相对于自身的坐标，这个point是经过放缩的
                GeneralTransform gt = container.TransformToAncestor(this);
                Point point = gt.Transform(new Point(0, 0));
                point.X *= -1;//需要负号
                point.Y *= -1;
                Point lt = point + selectBorder.GetLeftTop();
                Point rb = point + selectBorder.GetRightBottom();
                _viewModel.ClearSelection();
                List<NodeButton> newSelect = new List<NodeButton>();
                foreach (UserControl control in container.Children)
                {
                    if (control is NodeButton)
                    {
                        Vector ct = (control as NodeButton).GetCenter() * container.ScaleRatio;
                        //如果在selecBox内部
                        if (lt.X < ct.X && lt.Y < ct.Y
                            && rb.X > ct.X && rb.Y > ct.Y)
                        {
                            newSelect.Add(control as NodeButton);
                        }
                    }
                }
                selectBorder.Visibility = Visibility.Hidden;
                ViewModelFactory.FCC.NewSelection(newSelect);
            }
            if (this.Cursor == Cursors.Hand)
            {
                //控制滑动
                scrollViewer
                    .ScrollOffsetInertia(-_clickPoint.X + _oldclickPoint.X,
                    -_clickPoint.Y + _oldclickPoint.Y);
                this.Cursor = Cursors.Arrow;
            }
        }
        #endregion
    }
}