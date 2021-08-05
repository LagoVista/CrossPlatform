using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using SeaWolf.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class ConfigureAlertsViewModel : AppViewModelBase
    {
        private readonly IAppConfig _appConfig;

        public ConfigureAlertsViewModel(IAppConfig appConfig)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            Sensors.AddValidSensors(_appConfig, CurrentDevice);

            return base.InitAsync();
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        private KeyValuePair<string, object> _deviceParamer
        {
            get => new KeyValuePair<string, object>(nameof(Device), CurrentDevice);
        }

        public void EditConfig(SensorSummary sensorSummary)
        {
            ViewModelNavigation.NavigateAndEditAsync<ConfigureAlertViewModel>(this, sensorSummary.Config.Id, _deviceParamer,
                new KeyValuePair<string, object>(nameof(SensorSummary), sensorSummary));
        }

        public SensorSummary SelectedSensor
        {
            get => null;
            set
            {
                if(value != null)
                {
                    EditConfig(value);
                }
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<SensorSummary> Sensors { get; } = new ObservableCollection<SensorSummary>();
    }
}
