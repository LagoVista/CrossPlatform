using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DevicesViewModel : ListViewModelBase<DeviceSummary>
    {
        public DevicesViewModel()
        {
            //SeachNowCommand = RelayCommand<string>.Create((src) => SearchNow(src));
        }

        public void SearchNow(string str)
        {

        }

        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        protected override async void ItemSelected(DeviceSummary summary)
        {
            base.ItemSelected(summary);

            await DeviceSelectedAsync(summary);
        }

        private string _deviceFilter;
        public string DeviceFilter
        {
            get { return _deviceFilter; }
            set
            {
                Set(ref _deviceFilter, value);
            }
        }

        protected override string GetListURI()
        {
            return $"/api/devices/{LaunchArgs.ChildId}";
        }

        protected virtual async Task DeviceSelectedAsync(DeviceSummary summary)
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(DeviceViewModel),
                LaunchType = LaunchTypes.View
            };

            launchArgs.Parameters.Add(DeviceViewModelBase.DEVICE_ID, summary.Id);
            launchArgs.Parameters.Add(DeviceViewModelBase.DEVICE_REPO_ID, LaunchArgs.ChildId);

            SelectedItem = null;

            await ViewModelNavigation.NavigateAsync(launchArgs);
        }

        public ICommand SeachNowCommand { get; }
    }
}
