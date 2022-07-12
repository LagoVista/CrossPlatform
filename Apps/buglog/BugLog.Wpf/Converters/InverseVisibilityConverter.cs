using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace BugLog.Wpf.Converters
{
    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean)
            {
                return System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (value is IEnumerable<object> list)
            {
                return list.Any() ? Visibility.Collapsed : Visibility.Visible;
            }

            return value != null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
