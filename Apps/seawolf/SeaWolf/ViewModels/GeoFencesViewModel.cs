using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class GeoFencesViewModel : DeviceViewModelBase
    {
        public GeoFencesViewModel()
        {
            AddCommand = new RelayCommand(() => ViewModelNavigation.NavigateAndCreateAsync<GeoFenceViewModel>(this, DeviceLaunchArgsParam));
        }

        public override Task InitAsync()
        {            
            GeoFences = new ObservableCollection<GeoFence>(CurrentDevice.GeoFences);
            return base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            GeoFences = new ObservableCollection<GeoFence>(CurrentDevice.GeoFences);
            return base.ReloadedAsync();
        }

        ObservableCollection<GeoFence> _geoFences;
        public ObservableCollection<GeoFence> GeoFences
        {
            get => _geoFences;
            set => Set(ref _geoFences, value);
        }

        public void EditGeoFence(GeoFence geoFence)
        {
            ViewModelNavigation.NavigateAndEditAsync<GeoFenceViewModel>(this, geoFence.Id, DeviceLaunchArgsParam,
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

        public RelayCommand AddCommand { get; }

    }
}
