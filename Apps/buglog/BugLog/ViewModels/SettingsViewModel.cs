using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Client.Core.ViewModels.DeviceSetup;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugLog.ViewModels
{
    public class SettingsViewModel : AppViewModelBase
    {

        public SettingsViewModel()
        {
            MenuOptions = new List<MenuItem>()
            {
                new MenuItem() {FontIconKey = "fa-gear", Name = "Add New Boat", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<ConnectHardwareViewModel>(this)), Help="Add a new boar with BugLog Marine" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "Boats", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<MyDevicesViewModel>(this)), Help="My Boats" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "About", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<AboutViewModel>(this)), Help="My Boats" },
               /* new MenuItem() {FontIconKey = "fa-gear", Name = "Configure Geo Fences", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<GeoFencesViewModel>(this, _deviceParamer)), Help="Configure geofences for your vessel" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "Configure Alerts", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<ConfigureAlertsViewModel>(this,_deviceParamer)), Help="Set alerts" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "Add/Remove Sensors", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<SensorsViewModel>(this, _deviceParamer)), Help="Set alerts" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "WiFi Settings", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<BTDeviceScanViewModel>(this, _deviceParamer)), Help="Setup WiFi" },*/
            };
        }

        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        public List<MenuItem> MenuOptions { get; }
    }
}
