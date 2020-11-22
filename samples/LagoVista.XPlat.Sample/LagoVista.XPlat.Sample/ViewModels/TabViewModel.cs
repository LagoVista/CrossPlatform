using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LagoVista.XPlat.Sample.ViewModels
{
    public class TabViewModel : XPlatViewModel
    {
        public TabViewModel()
        {
            PropertiesTabActiveCommand = new RelayCommand(PropertiesTabActive);
        }

        public void PropertiesTabActive()
        {
            Debug.WriteLine("Prop Tab");
        }

        public RelayCommand PropertiesTabActiveCommand { get; }
    }
}
