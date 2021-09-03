using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceSetup
{
    public class SensorsViewModel : DeviceViewModelBase
    {
        public SensorsViewModel()
        {
            AddSensorCommand = new RelayCommand(ShowAddSensorView);
        }

        public async void ShowAddSensorView()
        {
            await ViewModelNavigation.NavigateAsync<SensorLibraryViewModel>(this, DeviceLaunchArgsParam);
        }

        public override Task InitAsync()
        {
            DeviceSensors = new ObservableCollection<Sensor>(CurrentDevice.SensorCollection);
            return base.InitAsync();
        }

        public ObservableCollection<Sensor> DeviceSensors { get; private set; }

        public RelayCommand AddSensorCommand{ get; }
    }
}
