using LagoVista.XPlat.Core.Services;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    public class Picker : Xamarin.Forms.Picker
    {
        public Picker()
        {
            FontFamily = ResourceSupport.GetString("EntryFont");
            FontSize = ResourceSupport.GetNumber("EntryFontSize");
            
            if (Device.RuntimePlatform != Device.UWP)
            {
                TextColor = ResourceSupport.GetColor("EditControlText");
                BackgroundColor = ResourceSupport.GetColor("EditControlBackground");
            }
        }
    }
}
