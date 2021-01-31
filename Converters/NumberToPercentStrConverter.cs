using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MultiBranchTexter.Converters
{
    //将数字转化为百分数字符串
    public class NumberToPercentStrConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double p = System.Convert.ToDouble(value);
            p *= 100;
            string r = Math.Round(p).ToString();
            return r + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
