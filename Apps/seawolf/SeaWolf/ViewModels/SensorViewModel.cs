using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
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
    public class SensorViewModel : AppViewModelBase
    {
        private readonly IDeviceManagementClient _deviceManagementClient;
        private readonly IAppConfig _appConfig;

        public SensorViewModel(IDeviceManagementClient deviceManagementClient, IAppConfig appConfig)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            SensorIndexes.Add(new KeyValuePair<int, string>(-1, "-select sensor slot-"));

            for (var idx = 1; idx <= 8; ++idx)
            {
                SensorIndexes.Add(new KeyValuePair<int, string>(idx, idx.ToString()));
            }

            SensorTypes.Add(new AppSpecificSensorTypes() { Key = "-1", Name = "-select sensor type-" });
            foreach (var snsr in _appConfig.AppSpecificSensorTypes)
                SensorTypes.Add(snsr);

            RemoveSensorCommand = new RelayCommand(RemoveSensor);
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));

            if (IsAdding)
            {
                SelectedSensorIndex = SensorIndexes[0];
                SelectedSensorType = SensorTypes[0];
            }
            else if (IsEditing)
            {
                var existingSummary = GetLaunchArg<SensorSummary>(nameof(SensorSummary));
                SensorName = existingSummary.Config.Name;
                SelectedSensorType = SensorTypes.FirstOrDefault(snsr => snsr.Key == existingSummary.Config.Key);
                SelectedSensorIndex = SensorIndexes.FirstOrDefault(snsr => snsr.Key == existingSummary.Config.SensorIndex);
                Description = existingSummary.Config.Description;
            }
            else
            {
                throw new InvalidOperationException($"Invalid launch type {LaunchArgs.LaunchType}.");
            }

            return base.InitAsync();
        }

        public async void RemoveSensor(Object job)
        {
            if ((await this.Popups.ConfirmAsync("Remove Sensor", "Are you sure?  This can not be undone.")))
            {
                var existingSummary = GetLaunchArg<SensorSummary>(nameof(SensorSummary));
                existingSummary.Config.Config = 0;
                existingSummary.Config.Description = null;
                existingSummary.Config.Name = null;
                existingSummary.Config.Key = null;
                existingSummary.Config.Zero = 0;
                existingSummary.Config.DeviceScaler = 1;
                existingSummary.Config.Calibration = 1;

                var result = await PerformNetworkOperation(async () =>
                {
                    return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
                });

                if (result.Successful)
                {
                    await this.ViewModelNavigation.GoBackAsync();
                }
            }
        }

        public async override void Save()
        {
            if (String.IsNullOrEmpty(SensorName))
            {
                await Popups.ShowAsync("Please specify a name for your sensor");
                return;
            }

            if (SelectedSensorIndex.Key == -1)
            {
                await Popups.ShowAsync("Please specify a slot for your sensor");
                return;
            }

            if (SelectedSensorType.Key == "-1")
            {
                await Popups.ShowAsync("Please specify the type of sensor.");
                return;
            }

            var portConfig = new PortConfig()
            {
                SensorIndex = SelectedSensorIndex.Key,
                Name = SensorName,
                Key = SelectedSensorType.Key,
                Description = Description,
                Config = (byte)SelectedSensorType.SensorConfigId
            };

            if (IsAdding)
            {
                portConfig.HighTheshold = SelectedSensorType.DefaultHighTolerance;
                portConfig.LowThreshold = SelectedSensorType.DefaultLowTolerance;
                portConfig.AlertsEnabled = true;
            }

            if (SelectedSensorType.Technology == SensorTechnology.ADC)
            {
                CurrentDevice.Sensors.AdcConfigs[portConfig.SensorIndex - 1] = portConfig;
            }
            else
            {
                CurrentDevice.Sensors.IoConfigs[portConfig.SensorIndex - 1] = portConfig;
            }

            var result = await PerformNetworkOperation(async () =>
            {
                return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
            });

            if (result.Successful)
            {
                await this.ViewModelNavigation.GoBackAsync();
            }
        }

        #region Properties
        public ObservableCollection<AppSpecificSensorTypes> SensorTypes { get; } = new ObservableCollection<AppSpecificSensorTypes>();
        public ObservableCollection<KeyValuePair<int, string>> SensorIndexes { get; } = new ObservableCollection<KeyValuePair<int, string>>();


        AppSpecificSensorTypes _selectedSensorType;
        public AppSpecificSensorTypes SelectedSensorType
        {
            get => _selectedSensorType;
            set => Set(ref _selectedSensorType, value);
        }

        KeyValuePair<int, string> _selectedSensorIndex;
        public KeyValuePair<int, string> SelectedSensorIndex
        {
            get => _selectedSensorIndex;
            set => Set(ref _selectedSensorIndex, value);
        }

        private string _sensorName;
        public string SensorName
        {
            get => _sensorName;
            set => Set(ref _sensorName, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }

        public RelayCommand RemoveSensorCommand { get; }
        #endregion
    }
}
