using LagoVista.XPlat.Core.Services;
using System.Drawing;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    public class Label : Xamarin.Forms.Label
    {
        public Label()
        {
            if (Device.RuntimePlatform != Device.UWP)
            {
                FontFamily = ResourceSupport.GetString("LabelFont");
                FontSize = ResourceSupport.GetNumber("LabelFontSize");
                TextColor = ResourceSupport.GetColor("LabelText");
            }
        }
    }
}
