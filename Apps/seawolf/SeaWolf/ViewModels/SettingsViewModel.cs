using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class SettingsViewModel : AppViewModelBase
    {

        public SettingsViewModel()
        {
            MenuOptions = new List<MenuItem>()
            {
                new MenuItem() {FontIconKey = "fa-gear", Name = "Provision Device", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<ProvisionDeviceViewModel>(this, _deviceParamer)), Help="Setup your device" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "Configure Geo Fences", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<GeoFencesViewModel>(this, _deviceParamer)), Help="Configure geofences for your vessel" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "Configure Alerts", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<ConfigureAlertsViewModel>(this,_deviceParamer)), Help="Set alerts" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "Add/Remove Sensors", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<SensorsViewModel>(this, _deviceParamer)), Help="Set alerts" },
                new MenuItem() {FontIconKey = "fa-gear", Name = "WiFi Settings", Command =  new RelayCommand(() => ViewModelNavigation.NavigateAsync<BTDeviceScanViewModel>(this, _deviceParamer)), Help="Setup WiFi" },
            };
        }

        private KeyValuePair<string, object> _deviceParamer
        {
            get => new KeyValuePair<string, object>(nameof(Device), CurrentDevice);
        }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            return base.InitAsync();
        }

        public List<MenuItem> MenuOptions { get; }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }
    }
}
