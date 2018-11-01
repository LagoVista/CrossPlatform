using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LagoVista.UWP.UI.Controls
{
    public class DialogFrame : Grid
    {
        public DialogFrame() : base()
        {
            Width = 400;
            Background = new SolidColorBrush(Colors.White);
            Padding = new Thickness(12);
            BorderBrush = new SolidColorBrush(Colors.Silver);
            BorderThickness = new Thickness(1);
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
        }
    }
}
