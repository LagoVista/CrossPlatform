using System;
using System.Globalization;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core.ValueConverter
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Xamarin.Forms.Color.Black;
            }

            switch (value.ToString().ToLower())
            {
                case "red": return Xamarin.Forms.Color.Red;
                case "brown": return Xamarin.Forms.Color.Brown;
                case "green": return Xamarin.Forms.Color.Green;
                case "blue": return Xamarin.Forms.Color.Blue;
                case "yellow": return Xamarin.Forms.Color.Yellow;
            }

            return Xamarin.Forms.Color.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RedGreenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(System.Convert.ToBoolean(value))
            {
                return Xamarin.Forms.Color.Green;
            }
            else
            {
                return Xamarin.Forms.Color.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
