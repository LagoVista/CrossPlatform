using System.Drawing;

namespace LagoVista.XPlat.Core
{
    public class Label : Xamarin.Forms.Label
    {
        public Label()
        {
            FontFamily = (string)Resources["LabelFont"];
            FontSize = (double)Resources["LabelFontSize"];
            TextColor = (Color)Resources["LabelText"];
        }
    }
}
