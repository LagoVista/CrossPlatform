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
        public SensorsViewModel()
        {
            AddSensorCommand = new RelayCommand(() => ViewModelNavigation.NavigateAndCreateAsync<SensorViewModel>(this,
                new KeyValuePair<string, object>("Device", CurrentDevice)));

            RemoveSensorCommand = RelayCommand<SensorSummary>.Create(RemoveSensor);
        }

        public RelayCommand AddSensorCommand { get; }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            Sensors.AddValidSensors(AppConfig, CurrentDevice);

            return base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            Sensors.AddValidSensors(AppConfig, CurrentDevice);

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
                return await DeviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
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