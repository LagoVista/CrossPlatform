using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class SensorViewModel : AppViewModelBase
    {
        public const string BATTERY = "battery";
        public const string MOTION = "motion";
        public const string MOISTURE = "moisture";
        public const string HIGHWATERLEVEL = "highwaterlevel";
        public const string BATTERYSWITCH = "batteryswitch";
        public const string AMBIENTTEMP = "ambienttemperature";
        public const string WATERTEMP = "watertemperature";

        private bool _isEditing;
        private bool _isADC;
        private int _originalSensorIndex = -1;

        private readonly IDeviceManagementClient _deviceManagementClient;

        public SensorViewModel(IDeviceManagementClient deviceManagementClient)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));

            SensorIndexes.Add(new KeyValuePair<int, string>(-1, "-selected sensor slot-"));

            for (var idx = 1; idx <= 8; ++idx)
            {
                SensorIndexes.Add(new KeyValuePair<int, string>(idx, idx.ToString()));
            }

            SensorTypes.Add(new KeyValuePair<string, string>("-1", "-selected sensor type-"));
            SensorTypes.Add(new KeyValuePair<string, string>(BATTERY, "Battery"));
            SensorTypes.Add(new KeyValuePair<string, string>(MOTION, "Motion"));
            SensorTypes.Add(new KeyValuePair<string, string>(MOISTURE, "Moisture"));
            SensorTypes.Add(new KeyValuePair<string, string>(HIGHWATERLEVEL, "High Water Level"));
            SensorTypes.Add(new KeyValuePair<string, string>(BATTERYSWITCH, "Battery Switch"));
            SensorTypes.Add(new KeyValuePair<string, string>(AMBIENTTEMP, "Water Temperature"));
            SensorTypes.Add(new KeyValuePair<string, string>(WATERTEMP, "Water Temperature"));
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public override Task InitAsync()
        {
            CurrentDevice = (Device)LaunchArgs.Parameters[nameof(Device)];
            if (CurrentDevice == null)
            {
                throw new ArgumentNullException(nameof(CurrentDevice));
            }

            if(!LaunchArgs.Parameters.ContainsKey("Action"))
            {
                throw new ArgumentNullException("Action is a required parameter to launch the sensor view mode,it should be either add or edit");
            }

            _isEditing = (string)LaunchArgs.Parameters["Action"] != "add";

            if (!_isEditing)
            {
                SelectedSensorIndex = _sensorIndexes[0];
                SelectedSensorType = SensorTypes[0];
            }
            else
            {
                if (!LaunchArgs.Parameters.ContainsKey("Index"))
                {
                    throw new ArgumentNullException("Index is a required parameter to launch the sensor view mode when editing, it should be the index of the sensor editing.");
                }

                if (!LaunchArgs.Parameters.ContainsKey("IOType"))
                {
                    throw new ArgumentNullException("IOType is a required parameter to launch the sensor view model when editing, it should be io or adc");
                }
                
                _isADC = (string)LaunchArgs.Parameters["IOType"] == "adc";
                _originalSensorIndex = int.Parse(LaunchArgs.Parameters["Index"].ToString());


                var portConfig = _isADC ? CurrentDevice.Sensors.AdcConfigs[_originalSensorIndex] : CurrentDevice.Sensors.IoConfigs[_originalSensorIndex];
                SensorName = portConfig.Name;
                SelectedSensorType = SensorTypes.FirstOrDefault(snsr => snsr.Key == portConfig.Key);
                SelectedSensorIndex = SensorIndexes.FirstOrDefault(snsr => snsr.Key == portConfig.SensorIndex);
                Description = portConfig.Description;
            }

            return base.InitAsync();
        }

        ObservableCollection<KeyValuePair<string, string>> _sensorTypes = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> SensorTypes
        {
            get => _sensorTypes;
        }

        ObservableCollection<KeyValuePair<int, string>> _sensorIndexes = new ObservableCollection<KeyValuePair<int, string>>();
        public ObservableCollection<KeyValuePair<int, string>> SensorIndexes
        {
            get => _sensorIndexes;
        }

        KeyValuePair<string, string> _selectedSensorType;
        public KeyValuePair<string, string> SelectedSensorType
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
                SensorIndex = SelectedSensorIndex.Key - 1,
                Name = SensorName,
                Key = SelectedSensorType.Key,
                Description = Description,
            };

            switch (SelectedSensorType.Key)
            {
                case BATTERY:
                    portConfig.Config = 1;
                    CurrentDevice.Sensors.AdcConfigs[portConfig.SensorIndex] = portConfig;
                    break;
                case MOTION:
                    portConfig.Config = 1;
                    CurrentDevice.Sensors.IoConfigs[portConfig.SensorIndex] = portConfig;
                    break;
                case HIGHWATERLEVEL:
                    portConfig.Config = 1;
                    CurrentDevice.Sensors.IoConfigs[portConfig.SensorIndex] = portConfig;
                    break;
                case BATTERYSWITCH:
                    portConfig.Config = 1;
                    CurrentDevice.Sensors.AdcConfigs[portConfig.SensorIndex] = portConfig;
                    break;
                case MOISTURE:
                    portConfig.Config = 1;
                    CurrentDevice.Sensors.IoConfigs[portConfig.SensorIndex] = portConfig;
                    break;
                case WATERTEMP:
                    portConfig.Config = 4;
                    CurrentDevice.Sensors.IoConfigs[portConfig.SensorIndex] = portConfig;
                    break;
                case AMBIENTTEMP:
                    portConfig.Config = 5;
                    CurrentDevice.Sensors.IoConfigs[portConfig.SensorIndex] = portConfig;
                    break;
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
    }
}
