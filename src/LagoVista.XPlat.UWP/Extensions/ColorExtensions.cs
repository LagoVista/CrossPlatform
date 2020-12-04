using LagoVista.XPlat.Core.Services;
using System.Drawing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace LagoVista.XPlat
{
    public static class ColorExtensions
    {
        public static void AddBrushResource(this ResourceDictionary targetDictionary, string name)
        {
            var sourceColor = ResourceSupport.GetColor(name);
            var uwpColor = Windows.UI.Color.FromArgb((byte)(sourceColor.A * 255), (byte)(sourceColor.R * 255), (byte)(sourceColor.G * 255), (byte)(sourceColor.B * 255));
            targetDictionary.Add(name, new SolidColorBrush(uwpColor));
        }

        public static Windows.UI.Color ToUWPColor(this Xamarin.Forms.Color sourceColor)
        {
            return Windows.UI.Color.FromArgb((byte)(sourceColor.A * 255), (byte)(sourceColor.R * 255), (byte)(sourceColor.G * 255), (byte)(sourceColor.B * 255));
        }
    }
}
