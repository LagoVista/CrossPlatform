using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DeviceViewModelBase : AppViewModelBase
    {
        private String _deviceRepoId;
        private String _deviceId;

        public const string DEVICE_ID = "DEVICE_ID";
        public const string DEVICE_REPO_ID = "DEVICE_REPO_ID";

        protected String DeviceId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceId))
                {
                    _deviceId = LaunchArgs.Parameters[DEVICE_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModel.DeviceId));
                }

                return _deviceId;
            }
        }

        protected string DeviceRepoId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceRepoId))
                {
                    _deviceRepoId = LaunchArgs.Parameters[DEVICE_REPO_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModel.DeviceRepoId));
                }

                return _deviceRepoId;
            }
        }

        protected void ShowView<TViewModel>()
        {
            var launchArgs = new ViewModelLaunchArgs()
            {
                ViewModelType = typeof(TViewModel),
                LaunchType = LaunchTypes.View
            };

            launchArgs.Parameters.Add(DEVICE_ID, DeviceRepoId);
            launchArgs.Parameters.Add(DEVICE_REPO_ID, DeviceId);

            ViewModelNavigation.NavigateAsync(launchArgs);
        }
    }
}
