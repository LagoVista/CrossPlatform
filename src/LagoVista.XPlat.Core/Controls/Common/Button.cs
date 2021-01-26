using LagoVista.XPlat.Core.Services;
using System.Windows.Input;

namespace LagoVista.XPlat.Core
{
    public class Button : Xamarin.Forms.Button
    {
        public Button()
        {
            BackgroundColor = ResourceSupport.GetColor("ButtonBackground");
            TextColor = ResourceSupport.GetColor("ButtonForeground");         
            FontSize = ResourceSupport.GetNumber("ButtonFontSize");            
        }
 
    }
}
