using LagoVista.XPlat.Core.Services;
using System.Windows.Input;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    public class Button : Xamarin.Forms.Button
    {
        public Button()
        {       
            if (Device.RuntimePlatform != Device.UWP)
            {
                FontSize = ResourceSupport.GetNumber("ButtonFontSize");
                BackgroundColor = ResourceSupport.GetColor("ButtonBackground");
                TextColor = ResourceSupport.GetColor("ButtonForeground");
            }
        }
 
    }
}
