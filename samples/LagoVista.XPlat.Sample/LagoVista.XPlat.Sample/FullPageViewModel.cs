using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample
{
    public class FullPageViewModel : XPlatViewModel
    {
        public FullPageViewModel()
        {

            LoginCommand = new RelayCommand(CloseScreen);
        }

        public RelayCommand LoginCommand { get; }
    }
}
