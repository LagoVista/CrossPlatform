using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Client.Core.ViewModels.DeviceSetup;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Geo;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class MainViewModel : DeviceViewModelBase
    {
        private bool _mainViewVisible = true;
        private bool _mapViewVisible;
        private bool _isNotFirstVessel;
        private bool _isNotLastVessel;

        Xamarin.Forms.Color _headerBackgroundColor = Xamarin.Forms.Color.FromRgb(0x55, 0xA9, 0xF2);
        Xamarin.Forms.Color _headerForegroundColor = Xamarin.Forms.Color.Black;

        private bool _alertsViewVisible;

        Device _currentDevice;
        ObservableCollection<DeviceSummary> _userDevices;

        public enum ViewToShow
        {
            Main,
            Map,
            Alerts
        }

        public MainViewModel()
        {
            MenuItems = new List<MenuItem>()
            {
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

            ViewMainCommand = new RelayCommand(() => ShowView(ViewToShow.Main));
            ViewMapCommand = new RelayCommand(() => ShowView(ViewToShow.Map));
            ViewAlertsCommand = new RelayCommand(() => ShowView(ViewToShow.Alerts));
            NextVesselCommand = new RelayCommand(NextVessel);
            PreviousVesselCommand = new RelayCommand(PreviousVessel);
            ViewSettingsCommand = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SettingsViewModel>(this, new KeyValuePair<string, object>(nameof(Device), CurrentDevice)));
        }

        public void ShowView(ViewToShow view)
        {
            MainViewVisible = false;
            AlertsViewVisible = false;
            MapViewVisible = false;

            switch (view)
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


        public async void NextVessel()
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));
            deviceIdx++;

            if (deviceIdx < UserDevices.Count)
            {
                DeviceId = UserDevices[deviceIdx].Id;
                await Storage.StoreKVP<string>(ComponentViewModel.DeviceId, DeviceId);
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
                await Storage.StoreKVP<string>(ComponentViewModel.DeviceId, DeviceId);
                await LoadDevice();
            }
        }

        public async override void Edit()
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(MyDeviceMenuViewModel),
                LaunchType = LaunchTypes.View
            };

            launchArgs.Parameters.Add(nameof(Device), CurrentDevice);
            await ViewModelNavigation.NavigateAsync(launchArgs);
        }

        private async Task<InvokeResult> LoadFromServer(bool busyFlag)
        {
            var response = await DeviceManagementClient.GetDevicesForUserAsync(AppConfig.DeviceRepoId, this.AuthManager.User.Id);
            if (!response.Successful)
            {
                return response.ToInvokeResult();
            }

            UserDevices = new ObservableCollection<DeviceSummary>(response.Model);
            await Storage.StoreKVP<ObservableCollection<DeviceSummary>>("USER_VESSELS", UserDevices);

            if (UserDevices.Count > 0)
            {
                HasDevices = true;
                NoDevices = !HasDevices;
                DeviceId = await Storage.GetKVPAsync<string>(ComponentViewModel.DeviceId);

                if (String.IsNullOrEmpty(DeviceId))
                {
                    DeviceId = UserDevices.First().Id;
                    await Storage.StoreKVP<string>(ComponentViewModel.DeviceId, DeviceId);
                }

                return await LoadDevice(busyFlag);
            }
            else
            {
                HasDevices = false;
                NoDevices = !HasDevices;
                return InvokeResult.Success;
            }
        }

        private async Task<InvokeResult> LoadFromCacheAsync()
        {
            DeviceId= await Storage.GetKVPAsync<string>(ComponentViewModel.DeviceId);

            if (!String.IsNullOrEmpty(DeviceId) && 
                await Storage.HasKVPAsync("USER_VESSELS") && await Storage.HasKVPAsync($"VESSEL_{DeviceId}"))
            {
                UserDevices = await Storage.GetKVPAsync<ObservableCollection<DeviceSummary>>("USER_VESSELS");
                CurrentDevice = await Storage.GetKVPAsync<Device>($"VESSEL_{DeviceId}");

                var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));
                IsNotLastVessel = deviceIdx < UserDevices.Count - 1;
                IsNotFirstVessel = deviceIdx > 0;

                if (CurrentDevice.GeoLocation != null)
                {
                    CurrentDeviceLocation = CurrentDevice.GeoLocation;
                }
                else
                {
                    /*    var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
                        if (lastKnownLocation != null)
                        {
                            CurrentVeseelLocation = new GeoLocation(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
                        }*/
                }

                Sensors = new ObservableCollection<Sensor>(CurrentDevice.SensorCollection);

                GeoFences.Clear();
                foreach (var geoFence in CurrentDevice.GeoFences)
                {
                    GeoFences.Add(geoFence);
                }

                await LoadFromServer(false);
                return InvokeResult.Success;
            }
            else
            {
                return await PerformNetworkOperation(async () =>
                 {
                     return await LoadFromServer(false);
                 });
            }
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
            await LoadFromCacheAsync();
        }

        protected override async Task DeviceLoadedAsync(Device device)
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));

            await Storage.StoreKVP<Device>($"VESSEL_{device.Id}", device);

            IsNotLastVessel = deviceIdx < UserDevices.Count - 1;
            IsNotFirstVessel = deviceIdx > 0;

            await base.DeviceLoadedAsync(device);
        }

        public override Task ReloadedAsync()
        {
            return LoadDevice();
        }


        #region Properties

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

        public Xamarin.Forms.Color HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set => Set(ref _headerBackgroundColor, value);
        }

        public Xamarin.Forms.Color HeaderForegroundColor
        {
            get => _headerForegroundColor;
            set => Set(ref _headerForegroundColor, value);
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
        #endregion

        #region Commands
        public RelayCommand<GeoLocation> MapTappedCommand { get; }
        public RelayCommand ViewMainCommand { get; }
        public RelayCommand ViewAlertsCommand { get; }
        public RelayCommand ViewMapCommand { get; }
        public RelayCommand ViewSettingsCommand { get; }

        public RelayCommand PreviousVesselCommand { get; }
        public RelayCommand NextVesselCommand { get; }

        #endregion
    }
}