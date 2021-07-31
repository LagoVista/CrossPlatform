using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Orgs;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Client.Devices;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Geo;
using LagoVista.Core.Validation;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SeaWolf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeaWolf.ViewModels
{
    public class DesignMockViewModel : MonitoringViewModelBase
    {
        private bool _isNotFirstVessel;
        private bool _isNotLastVessel;
        private bool _mainViewVisible = true;
        private bool _mapViewVisible;
        private bool _alertsViewVisible;
        GeoLocation _currentVeseelLocation;

        Device _currentDevice;
        ObservableCollection<DeviceSummary> _userDevices;

        public enum ViewToShow
        { 
            Main,
            Map,
            Alerts
        }

        public DesignMockViewModel()
        {
            MenuItems = new List<MenuItem>()
            {

                

                // -------------------------------------------------------------------------------------------------------
                // TODO: remove this when done with design & layout.
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<DesignMockViewModel>(this)),
                    Name = "Design Mock",
                    FontIconKey = "fa-gear"
                },
                // -------------------------------------------------------------------------------------------------------




                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SettingsViewModel>(this, new KeyValuePair<string, object>(nameof(Device), CurrentDevice))),
                    Name = "Settings",
                    FontIconKey = "fa-gear"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<AboutViewModel>(this)),
                    Name = "About",
                    FontIconKey = "fa-info"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => Logout()),
                    Name = ClientResources.Common_Logout,
                    FontIconKey = "fa-sign-out"
                }
            };

            NextVesselCommand = new RelayCommand(NextVessel);
            PreviousVesselCommand = new RelayCommand(PreviousVessel);

            ViewMainCommand = new RelayCommand(() => ShowView(ViewToShow.Main));
            ViewMapCommand = new RelayCommand(() => ShowView(ViewToShow.Map));
            ViewAlertsCommand = new RelayCommand(() => ShowView(ViewToShow.Alerts));
            ViewSettingsCommand = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SettingsViewModel>(this, new KeyValuePair<string, object>(nameof(Device), CurrentDevice)));
        }

        public void ShowView(ViewToShow view)
        {
            MainViewVisible = false;
            AlertsViewVisible = false;
            MapViewVisible = false;

            switch(view)
            {
                case ViewToShow.Main:
                    MainViewVisible = true;
                    break;
                case ViewToShow.Map:
                    MapViewVisible = true;
                    break;
                case ViewToShow.Alerts:
                    AlertsViewVisible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view), view, null);
            }
        }

        public override async Task InitAsync()
        {
            await PerformNetworkOperation(async () =>
            {
                var path = $"/api/devices/{AppConfig.DeviceRepoId}/{this.AuthManager.User.Id}";

                ListRestClient<DeviceSummary> _formRestClient = new ListRestClient<DeviceSummary>(RestClient);
                var result = await _formRestClient.GetForOrgAsync(path);
                if (!result.Successful)
                {
                    return result.ToInvokeResult();
                }

                UserDevices = new ObservableCollection<DeviceSummary>(result.Model);

                if (UserDevices.Count > 0)
                {
                    HasDevices = true;
                    NoDevices = !HasDevices;
                    DeviceId = await Storage.GetKVPAsync<string>(ComponentViewModel.DeviceId);

                    if (String.IsNullOrEmpty(DeviceId))
                    {
                        DeviceId = UserDevices.First().Id;
                    }                    

                    return await LoadDevice();
                }
                else
                {
                    HasDevices = false;
                    NoDevices = !HasDevices;
                    return InvokeResult.Success;
                }
            });
            await base.InitAsync();
        }

        private async Task<InvokeResult> LoadDevice()
        {
            return await PerformNetworkOperation(async () =>
            {
               CurrentDevice = null;
               var deviceResponse = await DeviceManagementClient.GetDeviceAsync(AppConfig.DeviceRepoId, DeviceId);
               if (deviceResponse.Successful)
               {
                   CurrentDevice = deviceResponse.Model;
                   var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));

                   IsNotLastVessel = deviceIdx < UserDevices.Count - 1;
                   IsNotFirstVessel = deviceIdx > 0;

                   if (CurrentDevice.GeoLocation != null)
                   {
                       CurrentVeseelLocation = CurrentDevice.GeoLocation;
                   }
                   else
                   {
                       //var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
                       //if (lastKnownLocation != null)
                       //{
                       //    CurrentVeseelLocation = new GeoLocation(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
                       //}
                   }

                   GeoFences.Clear();
                   foreach(var geoFence in CurrentDevice.GeoFences)
                   {
                       GeoFences.Add(geoFence);
                   }

                   Sensors.AddValidSensors(AppConfig, CurrentDevice);
               }

               return deviceResponse.ToInvokeResult();
           });
        }

        public override Task ReloadedAsync()
        {
            return LoadDevice();
        }

        public async void NextVessel()
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));
            deviceIdx++;

            if (deviceIdx < UserDevices.Count)
            {
                DeviceId = UserDevices[deviceIdx].Id;
                await LoadDevice();
            }
        }

        public async void PreviousVessel()
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));
            deviceIdx--;

            if (deviceIdx > -1)
            {
                DeviceId = UserDevices[deviceIdx].Id;
                await LoadDevice();
            }
        }

        #region Properties
        public ObservableCollection<GeoFence> GeoFences { get; } = new ObservableCollection<GeoFence>();

        private bool _hasDevices;
        public bool HasDevices
        {
            get { return _hasDevices; }
            set { Set(ref _hasDevices, value); }
        }

        private bool _noDevices;
        public bool NoDevices
        {
            get { return _noDevices; }
            set { Set(ref _noDevices, value); }
        }

        public ObservableCollection<SensorSummary> Sensors { get; } = new ObservableCollection<SensorSummary>();

        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public ObservableCollection<DeviceSummary> UserDevices
        {
            get => _userDevices;
            set => Set(ref _userDevices, value);
        }

        public bool IsNotFirstVessel
        {
            get => _isNotFirstVessel;
            set => Set(ref _isNotFirstVessel, value);
        }

        public bool IsNotLastVessel
        {
            get => _isNotLastVessel;
            set => Set(ref _isNotLastVessel, value);
        }    

        public GeoLocation CurrentVeseelLocation
        {
            get => _currentVeseelLocation;
            set => Set(ref _currentVeseelLocation, value);
        }
        
        public bool MainViewVisible
        {
            get => _mainViewVisible;
            set => Set(ref _mainViewVisible, value);
            
        }
        
        public bool MapViewVisible
        {
            get => _mapViewVisible;
            set => Set(ref _mapViewVisible, value);
            
        }

        public bool AlertsViewVisible
        {
            get => _alertsViewVisible;
            set => Set(ref _alertsViewVisible, value);
            
        }
        #endregion

        #region Commands
        public RelayCommand PreviousVesselCommand { get; }
        public RelayCommand NextVesselCommand { get; }
        public RelayCommand<GeoLocation> MapTappedCommand { get; }
        public RelayCommand ViewMainCommand { get; }
        public RelayCommand ViewAlertsCommand { get; }
        public RelayCommand ViewMapCommand { get; }
        public RelayCommand ViewSettingsCommand { get; }
        #endregion

        public override void HandleMessage(Notification notification)
        {
            switch(notification.PayloadType)
            {
                case nameof(Device):
                    var serializerSettings = new JsonSerializerSettings();
                    serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    var device = JsonConvert.DeserializeObject<Device>(notification.Payload, serializerSettings);
                    CurrentVeseelLocation = device.GeoLocation;

                    foreach (var sensor in Sensors)
                    {
                        if(sensor.SensorType.Technology == SensorTechnology.ADC)
                        {
                             
                        }
                    }


                    break;
            }
            var item = notification.Channel;
        }

        public override string GetChannelURI()
        {
            return $"/api/wsuri/device/{DeviceId}/normal";
        }
    }
}