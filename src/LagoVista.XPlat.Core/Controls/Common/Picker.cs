using LagoVista.XPlat.Core.Services;

namespace LagoVista.XPlat.Core
{
    public class Picker : Xamarin.Forms.Picker
    {
        public Picker()
        {
            FontFamily = ResourceSupport.GetString("EntryFont");
            FontSize = ResourceSupport.GetNumber("EntryFontSize");
            BackgroundColor = ResourceSupport.GetColor("EditControlBackground");
            TextColor = ResourceSupport.GetColor("EditControlText");
        }
    }
}
