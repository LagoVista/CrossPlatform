using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample
{
    public class ServicesViewModel : XPlatViewModel
    {
        public ServicesViewModel()
        {
            PromptforStringCommand = new RelayCommand(PromptForString);
        }

        public RelayCommand PromptforStringCommand { get; private set; } 

        public async void PromptForString()
        {
            var result = await  base.Popups.PromptForStringAsync("Give me a string!", isRequired:true);
            if(String.IsNullOrEmpty(result))
            {
                await Popups.ShowAsync("Cancelled or empty result");
            }
            else
            {
                await Popups.ShowAsync(result);
            }

        }
    }
}
