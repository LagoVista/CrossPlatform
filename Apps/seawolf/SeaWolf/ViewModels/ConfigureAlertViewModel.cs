using LagoVista.Client.Core.ViewModels;
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
            HighTolerance += .1;
        }

        public void IncrementLowThreshold(object obj)
        {
            LowTolerance += .1;
        }

        public void DecrementHighThreshold(object obj)
        {
            HighTolerance -= .1;
        }

        public void DecrementLowThreshold(object obj)
        {
            LowTolerance -= .1;
        }

        public bool CanIncrementHighThreshold(Object obj)
        {
            return true;
        }

        public bool CanIncrementLowThreshold(Object obj)
        {
            return HighTolerance > LowTolerance;
        }

        public bool CanDecrementHighThreshold(Object obj)
        {
            return HighTolerance > LowTolerance;
        }

        public bool CanDecrementLowThreshold(Object obj)
        {
            return true;
        }

        public double LowTolerance
        {
            get => _lowThreshold;
            set => Set(ref _lowThreshold, value);
        }

        public double HighTolerance
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

        public override void Save()
        {
            base.Save();
        }

    }
}
