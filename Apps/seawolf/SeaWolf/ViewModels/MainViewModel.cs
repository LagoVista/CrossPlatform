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
using SeaWolf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeaWolf.ViewModels
{
    public class MainViewModel : MonitoringViewModelBase
    {
        private bool _isNotFirstVessel;
        private bool _isNotLastVessel;


        private double _lowTemperatureThreshold = 80;
        private double _highTemperatureThreshold = 120;

        private double _lowBatteryThreshold = 12.5;
        private double _highBatteryThreshold = 14.5;

        EntityHeader _temperatureSensor;
        EntityHeader _batterySensor;

        GeoLocation _currentGeoFenceCenter;
        GeoLocation _currentVeseelLocation;


        Device _currentDevice;
        ObservableCollection<DeviceSummary> _userDevices;
        private readonly IDeviceManagementClient _deviceManagementClient;
        private readonly IAppConfig _appConfig;

        public MainViewModel(IDeviceManagementClient deviceManagementClient, IAppConfig appConfig)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SensorsViewModel>(this, new KeyValuePair<string, object>(nameof(Device), CurrentDevice))),
                    Name = "Sensors",
                    FontIconKey = "fa-users"
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

            IncrementHighBatteryThresholdCommand = new RelayCommand(IncrementHighBatteryThreshold, CanIncrementHighBatteryThreshold);
            IncrementLowBatteryThresholdCommand = new RelayCommand(IncrementLowBatteryThreshold, CanIncrementLowBatteryThreshold);
            DecrementHighBatteryThresholdCommand = new RelayCommand(DecrementHighBatteryThreshold, CanDecrementHighBatteryThreshold);
            DecrementLowBatteryThresholdCommand = new RelayCommand(DecrementLowBatteryThreshold, CanDecrementLowBatteryThreshold);

            IncrementHighTemperatureThresholdCommand = new RelayCommand(IncrementHighTemperatureThreshold, CanIncrementHighTemperatureThreshold);
            IncrementLowTemperatureThresholdCommand = new RelayCommand(IncrementLowTemperatureThreshold, CanIncrementLowTemperatureThreshold);
            DecrementHighTemperatureThresholdCommand = new RelayCommand(DecrementHighTemperatureThreshold, CanDecrementHighTemperatureThreshold);
            DecrementLowTemperatureThresholdCommand = new RelayCommand(DecrementLowTemperatureThreshold, CanDecrementLowTemperatureThreshold);

            MapTappedCommand = RelayCommand<GeoLocation>.Create(MapTapped);

            AddGeoFenceCommand = new RelayCommand(AddGeoFence);

            NextVesselCommand = new RelayCommand(NextVessel);
            PreviousVesselCommand = new RelayCommand(PreviousVessel);
        }

        public override async Task InitAsync()
        {
            await PerformNetworkOperation(async () =>
            {
                var path = $"/api/devices/{_appConfig.DeviceRepoId}/{this.AuthManager.User.Id}";

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

        private void AddSensors(IEnumerable<PortConfig> configs, double[] values)
        {
            foreach (var config in configs)
            {
                Sensors.Add(new SensorSummary()
                {
                    Config = config,
                    SensorType = _appConfig.AppSpecificSensorTypes.FirstOrDefault(sns => sns.Key == config.Key),
                    Value = values[config.SensorIndex - 1].ToString(),
                });
            }
        }

        private async Task<InvokeResult> LoadDevice()
        {
            return await PerformNetworkOperation(async () =>
           {
               CurrentDevice = null;
               var deviceResponse = await _deviceManagementClient.GetDeviceAsync(_appConfig.DeviceRepoId, DeviceId);
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
                       var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
                       if (lastKnownLocation != null)
                       {
                           CurrentVeseelLocation = new GeoLocation(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
                       }
                   }

                   if (CurrentDevice.GeoFences.Any())
                   {
                       CurrentGeoFenceCenter = CurrentDevice.GeoFences.First().Center;
                   }
                   else
                   {
                       CurrentGeoFenceCenter = CurrentVeseelLocation;
                   }

                   Sensors.Clear();

                   var configs = CurrentDevice.Sensors.AdcConfigs.Where(adc => adc.Config > 0);
                   AddSensors(configs, CurrentDevice.Sensors.AdcValues);
                 
                   configs = CurrentDevice.Sensors.IoConfigs.Where(io => io.Config > 0);
                   AddSensors(configs, CurrentDevice.Sensors.IoValues);
                   
      //             CurrentDevice.Sensors.BluetoothConfigs.Where(io => io.Config > 0);
               }

               return deviceResponse.ToInvokeResult();
           });
        }

        public void AddGeoFence()
        {

        }

        public void MapTapped(GeoLocation geoLocation)
        {
            CurrentGeoFenceCenter = geoLocation;
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

        public EntityHeader TemperatureSensor
        {
            get => _temperatureSensor;
            set
            {
                Set(ref _temperatureSensor, value);
                IncrementHighTemperatureThresholdCommand.RaiseCanExecuteChanged();
                IncrementLowTemperatureThresholdCommand.RaiseCanExecuteChanged();
                DecrementHighTemperatureThresholdCommand.RaiseCanExecuteChanged();
                DecrementLowTemperatureThresholdCommand.RaiseCanExecuteChanged();
            }
        }

        public EntityHeader BatterySensor
        {
            get => _batterySensor;
            set
            {
                Set(ref _batterySensor, value);
                IncrementHighBatteryThresholdCommand.RaiseCanExecuteChanged();
                IncrementLowBatteryThresholdCommand.RaiseCanExecuteChanged();
                DecrementHighBatteryThresholdCommand.RaiseCanExecuteChanged();
                DecrementLowBatteryThresholdCommand.RaiseCanExecuteChanged();
            }
        }

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

        public bool CanIncrementHighBatteryThreshold(Object obj)
        {
            return BatterySensor != null;
        }

        public bool CanIncrementLowBatteryThreshold(Object obj)
        {
            return BatterySensor != null && HighBatteryTolerance > LowBatteryTolerance;
        }

        public bool CanDecrementHighBatteryThreshold(Object obj)
        {
            return BatterySensor != null && HighBatteryTolerance > LowBatteryTolerance;
        }

        public bool CanDecrementLowBatteryThreshold(Object obj)
        {
            return BatterySensor != null;
        }

        public bool CanIncrementHighTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null;
        }

        public bool CanIncrementLowTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null && HighTemperatureTolerance > LowTemperatureTolerance;
        }

        public bool CanDecrementHighTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null && HighTemperatureTolerance > LowTemperatureTolerance;
        }

        public bool CanDecrementLowTemperatureThreshold(Object obj)
        {
            return TemperatureSensor != null;
        }

        public void IncrementHighBatteryThreshold(object obj)
        {
            HighBatteryTolerance += .1;
        }

        public void IncrementLowBatteryThreshold(object obj)
        {
            LowBatteryTolerance -= .1;
        }

        public void DecrementHighBatteryThreshold(object obj)
        {
            HighBatteryTolerance -= .1;
        }

        public void DecrementLowBatteryThreshold(object obj)
        {
            LowBatteryTolerance -= .1;
        }


        public void IncrementHighTemperatureThreshold(object obj)
        {
            HighTemperatureTolerance += 0.5;
        }

        public void IncrementLowTemperatureThreshold(object obj)
        {
            LowTemperatureTolerance += 0.5;
        }

        public void DecrementHighTemperatureThreshold(object obj)
        {
            HighTemperatureTolerance -= 0.5;
        }

        public void DecrementLowTemperatureThreshold(object obj)
        {
            LowTemperatureTolerance -= 0.5;
        }

        public double HighBatteryTolerance
        {
            get => _highBatteryThreshold;
            set => Set(ref _highBatteryThreshold, value);
        }
        public double LowBatteryTolerance
        {
            get => _lowBatteryThreshold;
            set => Set(ref _lowBatteryThreshold, value);
        }

        public double HighTemperatureTolerance
        {
            get => _highTemperatureThreshold;
            set => Set(ref _highTemperatureThreshold, value);
        }
        public double LowTemperatureTolerance
        {
            get => _lowTemperatureThreshold;
            set => Set(ref _lowTemperatureThreshold, value);
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

        public GeoLocation CurrentGeoFenceCenter
        {
            get => _currentGeoFenceCenter;
            set => Set(ref _currentGeoFenceCenter, value);
        }

        public GeoLocation CurrentVeseelLocation
        {
            get => _currentVeseelLocation;
            set => Set(ref _currentVeseelLocation, value);
        }

        public RelayCommand PreviousVesselCommand { get; }
        public RelayCommand NextVesselCommand { get; }

        public RelayCommand IncrementHighTemperatureThresholdCommand { get; }
        public RelayCommand IncrementLowTemperatureThresholdCommand { get; }

        public RelayCommand DecrementHighTemperatureThresholdCommand { get; }
        public RelayCommand DecrementLowTemperatureThresholdCommand { get; }

        public RelayCommand IncrementHighBatteryThresholdCommand { get; }
        public RelayCommand IncrementLowBatteryThresholdCommand { get; }

        public RelayCommand DecrementHighBatteryThresholdCommand { get; }
        public RelayCommand DecrementLowBatteryThresholdCommand { get; }

        public RelayCommand AddGeoFenceCommand { get; }


        public RelayCommand<GeoLocation> MapTappedCommand { get; }

        public override void HandleMessage(Notification notification)
        {
            throw new NotImplementedException();
        }

        public override string GetChannelURI()
        {
            return $"/api/wsuri/device/{DeviceId}/normal";
        }
    }
}
