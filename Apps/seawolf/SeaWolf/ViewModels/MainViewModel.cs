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

        public override async Task InitAsync()
        {
            await base.InitAsync();
            await PerformNetworkOperation(async () =>
            {
                var response = await DeviceManagementClient.GetDevicesForUserAsync(AppConfig.DeviceRepoId, this.AuthManager.User.Id);
                if(!response.Successful)
                {
                    return response.ToInvokeResult();
                }

                UserDevices = new ObservableCollection<DeviceSummary>(response.Model);

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
        }

        protected override Task DeviceLoadedAsync(Device device)
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));

            IsNotLastVessel = deviceIdx < UserDevices.Count - 1;
            IsNotFirstVessel = deviceIdx > 0;

            return base.DeviceLoadedAsync(device);
        }

        private async void SensorDetection()
        {
            Sensors.AddValidSensors(AppConfig, CurrentDevice);
            /*var outOfToleranceSensors = Sensors.Where(sns => sns.State == LagoVista.IoT.DeviceManagement.Models.SensorStates.Error);
            var warningSensors = Sensors.Where(sns => sns.State == LagoVista.IoT.DeviceManagement.Models.SensorStates.Warning);

            if (outOfToleranceSensors.Any())
            {
                HeaderBackgroundColor = Xamarin.Forms.Color.FromRgb(0xE9, 0x5C, 0x5D);
                HeaderForegroundColor = Xamarin.Forms.Color.White;
                SystemStatus = String.Join(" ", outOfToleranceSensors.Select(oot => oot.Config.Name + " " + oot.Value));
            }
            else if (warningSensors.Any())
            {
                HeaderBackgroundColor = Xamarin.Forms.Color.FromRgb(0xFF, 0xC8, 0x7F);
                HeaderForegroundColor = Xamarin.Forms.Color.White;
                SystemStatus = String.Join(" ", warningSensors.Select(oot => oot.Config.Name + " " + oot.Value));
            }
            else
            {
                HeaderBackgroundColor = Xamarin.Forms.Color.FromRgb(0x55, 0xA9, 0xF2);
                HeaderForegroundColor = Xamarin.Forms.Color.FromRgb(0x21, 0x21, 0x21);
                SystemStatus = "All systems nominal";
            }
            */
            await GattConnection.StartScanAsync();

            await SubscribeToWebSocketAsync();
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