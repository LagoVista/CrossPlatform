using LagoVista.XPlat.Core.Services;
using System.Drawing;

namespace LagoVista.XPlat.Core
{
    public class Label : Xamarin.Forms.Label
    {
        public Label()
        {
            FontFamily = ResourceSupport.GetString("LabelFont");
            FontSize = ResourceSupport.GetNumber("LabelFontSize");
            TextColor = ResourceSupport.GetColor("LabelText");
        }
    }
}
