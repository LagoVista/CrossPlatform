using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace LagoVista.UWP.UI
{
    public class EnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return false;

            if (parameter == null)
            {
                return !String.IsNullOrEmpty(value.ToString());
            }
            else
            {
                var minLength = System.Convert.ToInt32(parameter);
                return value.ToString().Length >= Math.Max(1, minLength);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
