using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                new MenuItem<FormControlsViewModel>(ViewModelNavigation, this) {FontIconKey = "fa-gear", Name = "Form Controls" },
                new MenuItem<DeviceSerialPortAccessViewModel>(ViewModelNavigation, this) {FontIconKey = "fa-gear", Name = "Serial Port" },
                new MenuItem<DFUViewModel>(ViewModelNavigation, this) {FontIconKey = "fa-gear", Name = "DFU View Model" },
                new MenuItem<ControlSampleViewModel>(ViewModelNavigation, this) {FontIconKey = "fa-gear", Name = "Control Examples" },
                new MenuItem() {FontIconKey = "fa-gear", Name ="Logout", Command = new RelayCommand(() => Logout())},
                new MenuItem() {FontIconKey = "fa-gear", Name = "Device Repos", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<DeviceReposViewModel>(this)) },
                new MenuItem<TabViewModel>(ViewModelNavigation, this) {FontIconKey = "fa-gear", Name= "Tabs"},
            };

            MenuItems = new ObservableCollection<MenuItem>()
            {
                 new MenuItem<TabViewModel>(ViewModelNavigation, this) {FontIconKey = "fa-gear", Name= "Tabs"},
            };
        }

        public List<MenuItem> MenuOptions {get;}
    }
}
