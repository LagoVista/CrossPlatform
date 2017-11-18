using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace LagoVista.UWP.UI
{
    public class TimePickerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is String) || value.ToString().Length != 5)
                throw new Exception("Time must be in HH:MM or 13:40");

            var hours = System.Convert.ToInt32(value.ToString().Substring(0, 2));
            var minutes = System.Convert.ToInt32(value.ToString().Substring(3, 2));

            return TimeSpan.FromMinutes(hours * 60 + minutes);

        }        

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is TimeSpan))
                throw new Exception("Value must be date-time");

            var dt = (TimeSpan)value;

            return String.Format("{0:00}:{1:00}", dt.Hours, dt.Minutes);
        }
    }
}
