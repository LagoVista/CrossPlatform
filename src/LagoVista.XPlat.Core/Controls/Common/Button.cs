using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{
    public class Button : Xamarin.Forms.Button
    {
        public Button()
        {
            BackgroundColor = (Color)Resources["ButtonBackground"];
            TextColor = (Color)Resources["ButtonForeground"];         
            
            FontSize = 16;            
        }

        public new ICommand Command
        {
            get { return base.Command; }
             set { base.Command = value; }
        }
        
    }
}
