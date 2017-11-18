using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using static LagoVista.Core.UWP.ViewModels.Common.NetworkSettingsViewModel;

namespace LagoVista.UWP.UI
{
    public class SectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var section = value as SettingsSection;
            if (section == null)
                return Visibility.Collapsed;

            section.IsVisible = (section.Key == parameter.ToString());
            return section.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
