﻿using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Devices;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using SeaWolf.Models;
using System;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class ConfigureAlertViewModel : AppViewModelBase
    {
        private readonly IDeviceManagementClient _deviceManagementClient;
        private readonly IAppConfig _appConfig;
        private SensorSummary _sensorSummary;

        private bool _isEnabled;
        private double _lowThreshold = 80;
        private double _highThreshold = 120;

        public ConfigureAlertViewModel(IDeviceManagementClient deviceManagementClient, IAppConfig appConfig)
        {
            _deviceManagementClient = deviceManagementClient ?? throw new ArgumentNullException(nameof(deviceManagementClient));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));

            IncrementHighThresholdCommand = new RelayCommand(IncrementHighThreshold, CanIncrementHighThreshold);
            IncrementLowThresholdCommand = new RelayCommand(IncrementLowThreshold, CanIncrementLowThreshold);
            DecrementHighThresholdCommand = new RelayCommand(DecrementHighThreshold, CanDecrementHighThreshold);
            DecrementLowThresholdCommand = new RelayCommand(DecrementLowThreshold, CanDecrementLowThreshold);
        }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            Sensor = GetLaunchArg<SensorSummary>(nameof(SensorSummary));

            HighThreshold = Sensor.Config.HighTheshold;
            LowThreshold = Sensor.Config.LowThreshold;
            IsEnabled = Sensor.Config.AlertsEnabled;

            return base.InitAsync();
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public void IncrementHighThreshold(object obj)
        {
            HighThreshold += .25;
        }

        public void IncrementLowThreshold(object obj)
        {
            LowThreshold += .25;
        }

        public void DecrementHighThreshold(object obj)
        {
            HighThreshold -= .25;
        }

        public void DecrementLowThreshold(object obj)
        {
            LowThreshold -= .25;
        }

        public bool CanIncrementHighThreshold(Object obj)
        {
            return true;
        }

        public bool CanIncrementLowThreshold(Object obj)
        {
            return HighThreshold > LowThreshold;
        }

        public bool CanDecrementHighThreshold(Object obj)
        {
            return HighThreshold > LowThreshold;
        }

        public bool CanDecrementLowThreshold(Object obj)
        {
            return true;
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => Set(ref _isEnabled, value);
        }

        public double LowThreshold
        {
            get => _lowThreshold;
            set => Set(ref _lowThreshold, value);
        }

        public double HighThreshold
        {
            get => _highThreshold;
            set => Set(ref _highThreshold, value);
        }

        public SensorSummary Sensor
        {
            get => _sensorSummary;
            set => Set(ref _sensorSummary, value);
        }

        public RelayCommand IncrementHighThresholdCommand { get; }
        public RelayCommand IncrementLowThresholdCommand { get; }

        public RelayCommand DecrementHighThresholdCommand { get; }
        public RelayCommand DecrementLowThresholdCommand { get; }

        public async override void Save()
        {            
            Sensor.Config.HighTheshold = HighThreshold;
            Sensor.Config.LowThreshold = LowThreshold;
            Sensor.Config.AlertsEnabled = IsEnabled;

            var result = await PerformNetworkOperation(async () =>
            {
                return await _deviceManagementClient.UpdateDeviceAsync(CurrentDevice.DeviceRepository.Id, CurrentDevice);
            });

            if (result.Successful)
            {
                await ViewModelNavigation.GoBackAsync();
            }
        }
    }
}
