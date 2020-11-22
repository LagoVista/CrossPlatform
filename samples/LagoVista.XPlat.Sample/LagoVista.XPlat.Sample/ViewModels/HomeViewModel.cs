using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.XPlat.Sample.ViewModels
{
    public class HomeViewModel : AppViewModelBase
    {

        public HomeViewModel()
        {
            MenuOptions = new List<MenuItem>()
            {
                new MenuItem() {FontIconKey = "fa-gear", Name = "Switch Orgs", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<UserOrgsViewModel>(this)) },
                new MenuItem() {FontIconKey = "fa-gear", Name ="Logout", Command = new RelayCommand(() => Logout())},
                new MenuItem() {FontIconKey = "fa-gear", Name = "Device Repos", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<DeviceReposViewModel>(this)) },
            };
        }

        public List<MenuItem> MenuOptions {get;}
    }
}
