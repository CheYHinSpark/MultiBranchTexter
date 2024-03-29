﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiBranchTexter.ViewModel;
using MultiBranchTexter.Resources;
using System.Windows.Interop;
using System.Windows.Shell;

namespace MultiBranchTexter.View
{
    public class MetroWindow : Window
    {
        #region 依赖属性
        public static DependencyProperty ShowSettingsProperty =
          DependencyProperty.Register("ShowSettings", typeof(bool),
              typeof(MetroWindow), new PropertyMetadata(false));

        public bool ShowSettings
        {
            get { return (bool)GetValue(ShowSettingsProperty); }
            set { SetValue(ShowSettingsProperty, value); }
        }

        #endregion

        #region 我也不懂这一坨东西是什么，但是它可以起到不遮盖任务栏的作用
        void Win_SourceInitialized(object sender, System.EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var handleSource = HwndSource.FromHwnd(handle);
            if (handleSource == null)
                return;
            handleSource.AddHook(WindowProc);
            this.SourceInitialized -= Win_SourceInitialized;
        }

        private static System.IntPtr WindowProc(
           System.IntPtr hwnd,
           int msg,
           System.IntPtr wParam,
           System.IntPtr lParam,
           ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = false;// <--让最小高宽生效
                    break;
            }
            return (System.IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            System.IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != System.IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top) - 1;
                //不知道为什么必须要有一个做出一点偏差才会生效
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            public RECT rcMonitor = new RECT();

            public RECT rcWork = new RECT();

            public int dwFlags;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public static readonly RECT Empty = new RECT();
        }
        #endregion

        public MetroWindow()
        {
            this.Loaded += MetroWindow_Loaded;
            this.SourceInitialized += Win_SourceInitialized;
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed
                && WindowState == WindowState.Maximized)
            {
                Point mousePt = Mouse.GetPosition(this);
                Point oldSize = new Point(this.ActualWidth, this.ActualHeight);
                WindowState = WindowState.Normal;
                Point newSize = new Point(this.ActualWidth, this.ActualHeight);
                this.Left = mousePt.X * (1 - newSize.X / oldSize.X);
                this.Top = mousePt.Y * (1 - newSize.Y / oldSize.Y);
                DragMove();

            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 查找窗体模板
            if (App.Current.Resources["MainWindowTemplate"] is ControlTemplate mWTemplate)
            {
                (mWTemplate.FindName("LayoutRoot", this) as Grid).DataContext = ViewModelFactory.Settings;
                (mWTemplate.FindName("SettingButton", this) as Button).Click += SettingBtn_Click;
                (mWTemplate.FindName("CloseWinButton", this) as Button).Click += CloseButton_Click;
                (mWTemplate.FindName("MinWinButton", this) as Button).Click += MinButton_Click;
                (mWTemplate.FindName("UpperBd", this) as Border).MouseDown += UpperBd_MouseDown;

                (mWTemplate.FindName("WindowTitleTbl", this) as TextBlock).Text = Title;
                (mWTemplate.FindName("titleBar", this) as Border).MouseMove += TitleBar_MouseMove;
            }
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
        }

        private void UpperBd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowSettings = false;
            ViewModelFactory.Settings.WriteIni();
        }

        private void SettingBtn_Click(object sender, RoutedEventArgs e)
        {
            SwitchShowOptions();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModelFactory.Settings.WriteIni();
            if (ViewModelFactory.Main.IsModified)
            {
                MessageBoxResult warnResult = MessageBox.Show
                    (
                    Application.Current.MainWindow,
                    LanguageManager.Instance["Msg_SaveFile"],
                    LanguageManager.Instance["Win_Warn"],
                    MessageBoxButton.YesNoCancel
                    );
                if (warnResult == MessageBoxResult.Yes)
                {
                    ViewModelFactory.Main.SaveFile();
                    Close();
                }
                else if (warnResult == MessageBoxResult.No)
                { Close(); }
            }
            else
            { Close(); }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // 实现窗体移动
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            DragMove();
            base.OnMouseLeftButtonDown(e);
        }

        public void SwitchShowOptions()
        { ShowSettings = !ShowSettings; }

        public void UpdateEffect()
        {
            if (ViewModelFactory.Settings.BlurOn)
            {
                // 实现透明模糊窗口需要 WindowChrome.GlassFrameThickness 至少一个分量为正
                (this.FindName("windowChrome") as WindowChrome).GlassFrameThickness = new Thickness(1);
                GlassHelper.EnableBlur(this, true);
            }
            else
            {
                GlassHelper.EnableBlur(this, false);
                // 实现透明窗口 需要 WindowChrome.GlassFrameThickness = 0
                (this.FindName("windowChrome") as WindowChrome).GlassFrameThickness = new Thickness(0);
                GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
            }
        }
    }
}
