using LagoVista.XPlat.Core.Services;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    public class Picker : Xamarin.Forms.Picker
    {
        public Picker()
        {
            if (Device.RuntimePlatform != Device.UWP)
            {
                FontFamily = ResourceSupport.GetString("EntryFont");
                FontSize = ResourceSupport.GetNumber("EntryFontSize");
                TextColor = ResourceSupport.GetColor("EditControlText");
                BackgroundColor = ResourceSupport.GetColor("EditControlBackground");
            }
        }
    }
}
