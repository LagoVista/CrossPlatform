using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DevicesViewModel : ListViewModelBase<DeviceSummary>
    {
        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        protected override async void ItemSelected(DeviceSummary summary)
        {
            base.ItemSelected(summary);

            await DeviceSelectedAsync(summary);
        }

        private bool _hasDevices;
        public bool HasDevices
        {
            get { return _hasDevices; }
            set { Set(ref _hasDevices, value); }
        }

        private bool _noDevices;
        public bool NoDevices
        {
            get { return _noDevices; }
            set { Set(ref _noDevices, value); }
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

            launchArgs.Parameters.Add(DeviceViewModelBase.DEVICE_ID, LaunchArgs.ChildId);
            launchArgs.Parameters.Add(DeviceViewModelBase.DEVICE_REPO_ID, summary.Id);

            SelectedItem = null;

            await ViewModelNavigation.NavigateAsync(launchArgs);
        }
    }
}
