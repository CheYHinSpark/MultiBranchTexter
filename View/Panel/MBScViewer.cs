using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiBranchTexter.ViewModel;

namespace MultiBranchTexter.View
{
    /// <summary>
    /// 惯性ScrollViewer
    /// </summary>
    public class MBScViewer : ScrollViewer
    {
        //是否启用惯性
        //public static DependencyProperty IsEnableInertiaProperty =
        //    DependencyProperty.Register("IsEnableInertia", //属性名称
        //        typeof(bool), //属性类型
        //        typeof(MBScViewer), //该属性所有者，即将该属性注册到那个类上
        //        new PropertyMetadata(true)//属性默认值
        //        );

        //public bool IsEnableInertia
        //{
        //    get { return (bool)GetValue(IsEnableInertiaProperty); }
        //    set { SetValue(IsEnableInertiaProperty, value); }
        //}

        private bool _isRunning = false;
        private bool _stopOldOne = false;
        private double _targetVOffset;
        private double _targetHOffset;

        public MBScViewer() { }

        //禁用原有的方法
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!ViewModelFactory.Settings.IsEnableInertia)
            {
                base.OnMouseWheel(e);
                return;
            }
            e.Handled = true;
            _targetVOffset = -e.Delta * 0.2;
            if (!_isRunning)
            { ScrollToTargetInertia(); }
        }

        public async void ScrollToTargetInertia()
        {
            _isRunning = true;
            bool h = Math.Abs(_targetHOffset) > 0.5;
            bool v = Math.Abs(_targetVOffset) > 0.5;
            while (h || v)
            {
                if (_stopOldOne)
                { 
                    _stopOldOne = false;
                    _isRunning = false;
                    return; 
                }
                if (h)
                { ScrollToHorizontalOffset(_targetHOffset + HorizontalOffset); }
                if (v)
                { ScrollToVerticalOffset(_targetVOffset + VerticalOffset); }
                await Task.Delay(16);
                _targetHOffset -= Math.Sign(_targetHOffset);
                _targetVOffset -= Math.Sign(_targetVOffset);
                h = Math.Abs(_targetHOffset) > 0.5;
                if (!h)
                { _targetHOffset = 0; }
                v = Math.Abs(_targetVOffset) > 0.5;
                if (!v)
                { _targetVOffset = 0; }
            }
            _isRunning = false;
            _stopOldOne = false;
        }

        public void ScrollOffsetInertia(double x, double y)
        {
            if (!ViewModelFactory.Settings.IsEnableInertia)
            { return; }
            //如果偏差很小，就忽略之，否则强化偏差
            if (Math.Abs(x) < 2 && Math.Abs(y) < 2)
            { return; }
            _stopOldOne = false;
            _targetVOffset = y;
            _targetHOffset = x;
            //如果正在滚动，只需要修改target值即可
            if (_isRunning)
            { return; }
            ScrollToTargetInertia();
        }

        public void StopInertia()
        { _stopOldOne = true; }
    }
}
