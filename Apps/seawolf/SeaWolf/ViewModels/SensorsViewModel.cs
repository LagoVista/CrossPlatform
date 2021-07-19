using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using SeaWolf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class SensorsViewModel : AppViewModelBase
    {
        private readonly IDeviceManagementClient _deviceManagementClient;
        private readonly IAppConfig _appConfig;

        public SensorsViewModel(IDeviceManagementClient deviceManagementClient, IAppConfig appConfig)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            AddSensorCommand = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SensorViewModel>(this,
                new KeyValuePair<string, object>("Device", CurrentDevice),
                new KeyValuePair<string, object>("Action", "add")));

            RemoveSensorCommand = RelayCommand<SensorSummary>.Create(RemoveSensor);
        }

        public RelayCommand AddSensorCommand { get; }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            Sensors.AddValidSensors(_appConfig, CurrentDevice);

            return base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            Sensors.AddValidSensors(_appConfig, CurrentDevice);

            return base.ReloadedAsync();
        }

        public async  void RemoveSensor(object obj)
        {
            var cfg = obj as SensorSummary;
            Sensors.Remove(cfg);
            cfg.Config.Config = 0;
            cfg.Config.Description = null;
            cfg.Config.Name = null;
            cfg.Config.Key = null;
            cfg.Config.Zero = 0;
            cfg.Config.DeviceScaler = 1;
            cfg.Config.Calibration = 1;

            var result = await PerformNetworkOperation(async () =>
            {
                return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
            });
        }

        public ObservableCollection<SensorSummary> Sensors { get; } = new ObservableCollection<SensorSummary>();

        public SensorSummary SelectedSensor
        {
            get => null;
            set
            {
                if (value != null)
                {
                    ViewModelNavigation.NavigateAndEditAsync<SensorViewModel>(this,
                        value.Config.Id,
                        new KeyValuePair<string, object>(nameof(Device), CurrentDevice),
                        new KeyValuePair<string, object>(nameof(SensorSummary), value));
                }

                RaisePropertyChanged();
            }
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public RelayCommand<SensorSummary> RemoveSensorCommand { get; }
    }
}