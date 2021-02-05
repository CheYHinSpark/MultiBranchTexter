using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;

namespace MultiBranchTexter.Converters
{
  
    //将List<NodeButton>转化为Bool
    public class ListToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = 0;
            if (parameter != null)
            { i++; }
            List<NodeButton> nbs = value as List<NodeButton>;
            return (nbs.Count > i);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
