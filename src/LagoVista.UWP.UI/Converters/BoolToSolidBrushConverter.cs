using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace LagoVista.UWP.UI
{
    public class BoolToSolidBrushConverter : IValueConverter
    {
        /// <summary>
        /// Convert from source-type to target-type
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            return new SolidColorBrush(Windows.UI.Color.FromArgb(System.Convert.ToByte((bool)value ? 128 : 255), 128, 128, 128));
        }

        /// <summary>
        /// Convert-back from target to source.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            return null;
        }
    }

}
