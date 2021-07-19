using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class GeoFencesViewModel : AppViewModelBase
    {
        public GeoFencesViewModel()
        {
            AddCommand = new RelayCommand(() => ViewModelNavigation.NavigateAndCreateAsync<GeoFenceViewModel>(this, _deviceParamer));
        }

        public override Task InitAsync()
        {
            CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            GeoFences = new ObservableCollection<GeoFence>(CurrentDevice.GeoFences);

            return base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            GeoFences = new ObservableCollection<GeoFence>(CurrentDevice.GeoFences);

            return base.ReloadedAsync();
        }

        private KeyValuePair<string, object> _deviceParamer
        {
            get => new KeyValuePair<string, object>(nameof(Device), CurrentDevice);
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set => Set(ref _currentDevice, value);
        }

        public RelayCommand AddCommand { get; }

        ObservableCollection<GeoFence> _geoFences;
        public ObservableCollection<GeoFence> GeoFences
        {
            get => _geoFences;
            set => Set(ref _geoFences, value);
        }

        public void EditGeoFence(GeoFence geoFence)
        {
            ViewModelNavigation.NavigateAndEditAsync<GeoFenceViewModel>(this, geoFence.Id, _deviceParamer,
                new KeyValuePair<string, object>(nameof(GeoFence), geoFence));
        }

        public GeoFence SelectedGeoFence
        {
            get => null;
            set
            {
                if (value != null)
                {
                    EditGeoFence(value);
                }
                RaisePropertyChanged();
            }
        }

    }
}
