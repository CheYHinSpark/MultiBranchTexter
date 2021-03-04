using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using MultiBranchTexter.View;

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
            ObservableCollection<NodeButton> nbs = value as ObservableCollection<NodeButton>;
            return (nbs.Count > i);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
