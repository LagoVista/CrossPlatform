using System;
using System.Globalization;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.ValueConverter
{
    public class DatePrinterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (String.IsNullOrEmpty(strValue) && DateTime.TryParse(strValue, out DateTime dateValue))
                {
                    return dateValue.ToString();
                }
            }

            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
