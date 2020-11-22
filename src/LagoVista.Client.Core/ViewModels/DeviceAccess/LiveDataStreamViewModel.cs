using LagoVista.Client.Core.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class LiveDataStreamViewModel : MonitoringViewModelBase
    {
        public const string DeviceId = "DEVICE_ID";
        public const string DeviceRepoId = "DEVICE_REPO_ID";

        private String _deviceRepoId;
        private String _deviceId;

        Device _device;

        public override async Task InitAsync()
        {
            await base.InitAsync();

            IsBusy = true;

            _deviceRepoId = LaunchArgs.Parameters[LiveDataStreamViewModel.DeviceRepoId].ToString();
            _deviceId = LaunchArgs.Parameters[LiveDataStreamViewModel.DeviceId].ToString();
        }

        public override string GetChannelURI()
        {
            return $"/api/wsuri/device/{_deviceId}/normal";
        }

        public override void HandleMessage(Notification notification)
        {
            if (!String.IsNullOrEmpty(notification.PayloadType))
            {
                switch (notification.PayloadType)
                {
                    case "DeviceArchive":
                        var archive = JsonConvert.DeserializeObject<DeviceArchive>(notification.Payload);
                        DispatcherServices.Invoke(() =>
                        {
                            DeviceMessages.Insert(0, archive);
                            if (DeviceMessages.Count == 50)
                            {
                                DeviceMessages.RemoveAt(2);
                            }
                        });
                        break;
                    case "Device":
                        DispatcherServices.Invoke(() =>
                        {
                            Device = JsonConvert.DeserializeObject<Device>(notification.Payload);
                        });

                        break;
                }
                Debug.WriteLine("----");
                Debug.WriteLine(notification.PayloadType);
                Debug.WriteLine(notification.Payload);
                Debug.WriteLine("BYTES: " + notification.Payload.Length);
                Debug.WriteLine("----");
            }
            else
            {
                Debug.WriteLine(notification.Text);
            }
        }

        private ObservableCollection<DeviceArchive> _deviceMessasges;
        public ObservableCollection<DeviceArchive> DeviceMessages
        {
            get { return _deviceMessasges; }
            set { Set(ref _deviceMessasges, value); }
        }

        public Device Device
        {
            get { return _device; }
            set
            {
                Set(ref _device, value);
            }
        }
    }
}
