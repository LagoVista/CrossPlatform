using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace LagoVista.UWP.UI
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Int64 colorValue;
            if (value is System.String)
            {
                switch(((string)value).ToLower())
                {
                    case "red": colorValue = 0xFF0000; break;
                    case "green": colorValue = 0x007F00; break;
                    case "blue": colorValue = 0x00007F; break;
                    case "yellow": colorValue = 0xFFFF00; break;
                    case "white": colorValue = 0xFFFFFF; break;
                    case "black": colorValue = 0x000000; break;
                    default:
                        colorValue = System.Convert.ToInt32((value as string).Substring(1), 16);
                        break;
                }
                
            }
                
            else
                colorValue = (long)value;

            var a = System.Convert.ToByte((colorValue >> 24) & 0xFF);
            if (a == 0) a = 0xFF;
            var r = System.Convert.ToByte((colorValue >> 16) & 0xFF);
            var g = System.Convert.ToByte((colorValue >> 8) & 0xFF);
            var b = System.Convert.ToByte(colorValue & 0xFF);

            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}