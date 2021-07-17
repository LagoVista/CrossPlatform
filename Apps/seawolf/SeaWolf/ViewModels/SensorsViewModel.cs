using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
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

        public SensorsViewModel(IDeviceManagementClient deviceManagementClient)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));

            AddSensorCommand = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SensorViewModel>(this,
                new KeyValuePair<string, object>("Device", CurrentDevice),
                new KeyValuePair<string, object>("Action", "add")));

            RemoveAdcConfigCommand = new RelayCommand(RemoveAdcConfig);
            RemoveIoConfigCommand = new RelayCommand(RemoveIoConfig);
        }

        public RelayCommand AddSensorCommand { get; }

        public override Task InitAsync()
        {
            CurrentDevice = (Device)LaunchArgs.Parameters[nameof(Device)];
            if (CurrentDevice == null)
            {
                throw new ArgumentNullException(nameof(CurrentDevice));
            }

            AdcConfigs = new ObservableCollection<PortConfig>(CurrentDevice.Sensors.AdcConfigs.Where(adc => adc.Config > 0));
            IoConfigs = new ObservableCollection<PortConfig>(CurrentDevice.Sensors.IoConfigs.Where(io => io.Config > 0));

            return base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            AdcConfigs = new ObservableCollection<PortConfig>(CurrentDevice.Sensors.AdcConfigs.Where(adc => adc.Config > 0));
            IoConfigs = new ObservableCollection<PortConfig>(CurrentDevice.Sensors.IoConfigs.Where(io => io.Config > 0));

            return base.ReloadedAsync();
        }

        public async void RemoveAdcConfig(object obj)
        {
            var cfg = obj as PortConfig;
            AdcConfigs.Remove(cfg);
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].Config = 0;
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].Description = null;
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].Name = null;
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].Key = null;
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].Zero = 0;
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].DeviceScaler = 1;
            CurrentDevice.Sensors.AdcConfigs[cfg.SensorIndex].Calibration = 1;

            var result = await PerformNetworkOperation(async () =>
            {
                return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
            });
        }

        public async  void RemoveIoConfig(object obj)
        {
            var cfg = obj as PortConfig;
            IoConfigs.Remove(cfg);
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].Config = 0;
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].Description = null;
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].Name = null;
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].Key = null;
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].Zero = 0;
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].DeviceScaler = 1;
            CurrentDevice.Sensors.IoConfigs[cfg.SensorIndex].Calibration = 1;

            var result = await PerformNetworkOperation(async () =>
            {
                return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
            });         
        }

        ObservableCollection<PortConfig> _adcConfigs;
        public ObservableCollection<PortConfig> AdcConfigs
        {
            get => _adcConfigs;
            set => Set(ref _adcConfigs, value);
        }

        ObservableCollection<PortConfig> _ioConfigs;
        public ObservableCollection<PortConfig> IoConfigs
        {
            get => _ioConfigs;
            set => Set(ref _ioConfigs, value);
        }

        public PortConfig SelectedAnalogConfig
        {
            get => null;
            set
            {
                if (value != null)
                {
                    ViewModelNavigation.NavigateAsync<SensorViewModel>(this,
                        new KeyValuePair<string, object>("Device", CurrentDevice),
                        new KeyValuePair<string, object>("IOType", "adc"),
                        new KeyValuePair<string, object>("Index", value.SensorIndex),
                        new KeyValuePair<string, object>("Action", "edit"));
                }

                RaisePropertyChanged();
            }
        }

        public PortConfig SelectedDigitalConfig
        {
            get => null;
            set
            {
                if (value != null)
                {
                    ViewModelNavigation.NavigateAsync<SensorViewModel>(this,
                        new KeyValuePair<string, object>("Device", CurrentDevice),
                        new KeyValuePair<string, object>("IOType", "gpio"),
                        new KeyValuePair<string, object>("Index", value.SensorIndex),
                        new KeyValuePair<string, object>("Action", "edit"));
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

        public RelayCommand RemoveIoConfigCommand { get; }
        public RelayCommand RemoveAdcConfigCommand { get; }
    }
}