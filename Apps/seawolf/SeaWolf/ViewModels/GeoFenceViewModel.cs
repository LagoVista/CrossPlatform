using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.Geo;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SeaWolf.ViewModels
{
    public class GeoFenceViewModel : AppViewModelBase
    {
        private readonly IDeviceManagementClient _deviceManagementClient;
        private readonly IAppConfig _appConfig;
        private GeoFence _geoFence;
        double _geoFenceRadiusMeters = 500;
        GeoLocation _geoFenceCenter;
        GeoLocation _mapCenter;
        string _geoFenceName;
        bool _isGeoFenceEnabled;
        string _geoFenceDescription;

        public GeoFenceViewModel(IDeviceManagementClient deviceManagementClient, IAppConfig appConfig)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            MapTappedCommand = RelayCommand<GeoLocation>.Create(MapTapped);
        }

        public override async Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            if (IsAdding)
            {
                GeoFence = new GeoFence();
                var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
                if (lastKnownLocation != null)
                {
                    GeoFenceCenter = new GeoLocation(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
                    MapCenter = GeoFenceCenter;
                    GeoFenceRadiusMeters = 500;
                    IsGeoFenceEnabled = true;
                }
                else
                {
                    await Popups.ShowAsync("Could not find current location, please allow location services.");
                    await ViewModelNavigation.GoBackAsync();
                }

                GeoFenceDescription = string.Empty;
            }
            else if(IsEditing)
            {
                GeoFence = GetLaunchArg<GeoFence>(nameof(GeoFence));
                IsGeoFenceEnabled = GeoFence.Enabled;
                GeoFenceName = GeoFence.Name;
                GeoFenceRadiusMeters = GeoFence.RadiusMeters;
                GeoFenceCenter = GeoFence.Center;
                MapCenter = GeoFence.Center;
                GeoFenceDescription = GeoFence.Description;

            }
            else
            {
                throw new InvalidOperationException($"Don't know how to {LaunchArgs.LaunchType}");
            }

            await base.InitAsync();
        }

        public void MapTapped(GeoLocation geoLocation)
        {
            MapCenter = geoLocation;
            GeoFenceCenter = geoLocation;
        }

        public override async void Save()
        {
            GeoFence.Name = GeoFenceName;
            GeoFence.RadiusMeters = GeoFenceRadiusMeters;
            GeoFence.Enabled = IsGeoFenceEnabled;
            GeoFence.Center = GeoFenceCenter;
            GeoFence.Description = GeoFenceDescription;

            if(IsAdding)
            {
                CurrentDevice.GeoFences.Add(GeoFence);
            }

            var result = await PerformNetworkOperation(async () =>
            {
                return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
            });

            if (result.Successful)
            {
                await ViewModelNavigation.GoBackAsync();
            }
        }

        public GeoFence GeoFence
        {
            get => _geoFence;
            set => Set(ref _geoFence, value);
        }

        public double GeoFenceRadiusMeters
        {
            get => _geoFenceRadiusMeters;
            set => Set(ref _geoFenceRadiusMeters, value);
        }

        public string GeoFenceName
        {
            get => _geoFenceName;
            set => Set(ref _geoFenceName, value);
        }

        public bool IsGeoFenceEnabled
        {
            get => _isGeoFenceEnabled;
            set => Set(ref _isGeoFenceEnabled, value);
        }

        public GeoLocation MapCenter
        {
            get => _mapCenter;
            set => Set(ref _mapCenter, value);
        }

        public GeoLocation GeoFenceCenter
        {
            get => _geoFenceCenter;
            set => Set(ref _geoFenceCenter, value);
        }

        public string GeoFenceDescription
        {
            get => _geoFenceDescription;
            set => Set(ref _geoFenceDescription, value);
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public RelayCommand<GeoLocation> MapTappedCommand { get; }
    }
}
