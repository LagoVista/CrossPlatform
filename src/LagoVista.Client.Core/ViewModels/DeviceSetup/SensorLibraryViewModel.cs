﻿using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceSetup
{
    public class SensorLibraryViewModel : DeviceViewModelBase
    {
        public override async Task InitAsync()
        {
            await PerformNetworkOperation(async () =>
            {
                var deviceConfig = await DeviceManagementClient.GetDeviceConfigurationAsync(CurrentDevice.DeviceConfiguration.Id);
                SensorTypes = new ObservableCollection<SensorDefinition>(deviceConfig.SensorDefinitions);
            });

            await base.InitAsync();
        }

        private async void AddSensorAsync(SensorDefinition definition)
        {
            var newSensor = new Sensor(definition);

            CurrentDevice.SensorCollection.Add(newSensor);

            var result = await PerformNetworkOperation(async () =>
            {
                return await DeviceManagementClient.UpdateDeviceAsync(AppConfig.DeviceRepoId, CurrentDevice);
            });


            if (result.Successful)
            {
                await ViewModelNavigation.NavigateAsync<SensorsViewModel>(this, DeviceLaunchArgsParam, CreateKVP(SensorDetailViewModel.SENSOR_ID, newSensor.Id));
            }

        }

        SensorDefinition _selectedSensor;
        public SensorDefinition SelectedSensor
        {
            get { return _selectedSensor; }
            set
            {
                Set(ref _selectedSensor, value);
                AddSensorAsync(value);
            }
        }

        private ObservableCollection<SensorDefinition> _sensorTypes;
        public ObservableCollection<SensorDefinition> SensorTypes
        {
            get => _sensorTypes;
            private set
            {
                Set(ref _sensorTypes, value);
            }
        }
    }
}