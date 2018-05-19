using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LagoVista.XPlat.Sample
{
    public class ControlSampleViewModel : XPlatViewModel
    {
        public ControlSampleViewModel()
        {
            IconButtonTapCommand = new RelayCommand(IconButtonTap);

        }

        public void IconButtonTap(Object obj)
        {
            Debug.WriteLine("I am tapped");
            Debug.WriteLine(obj.ToString());
        }

        public RelayCommand IconButtonTapCommand { get; private set; }


        public string MyValue { get { return "Testing...one...two...three"; } }
    }
}
