using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace MultiBranchTexter.Converters
{
    public class WindowStateToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            WindowState ws = (WindowState)value;
            if (ws == WindowState.Maximized)
            { return true; }
            else
            { return false; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == true)
            { return WindowState.Maximized; }
            else
            { return WindowState.Normal; }
        }
    }
}
