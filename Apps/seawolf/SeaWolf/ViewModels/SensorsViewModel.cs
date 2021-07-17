using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class SensorsViewModel : AppViewModelBase
    {
        public SensorsViewModel()
        {
            AddSensorCommand = new RelayCommand(() => ViewModelNavigation.NavigateAsync<SensorViewModel>(this,
                new KeyValuePair<string, object>("Device", CurrentDevice),
                new KeyValuePair<string, object>("Action", "add")));
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
    }
}