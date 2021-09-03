using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceSetup
{
    public class PairHardwareViewModel : DeviceViewModelBase
    {
        public PairHardwareViewModel()
        {

        }

        public ObservableCollection<BLEDevice> DiscoveredDevices { get; private set; }

        public async override Task InitAsync()
        {
            await GattConnection.StartScanAsync();
            DiscoveredDevices = GattConnection.DiscoveredDevices;

            await base.InitAsync();
        }

        public void Next()
        {
            ViewModelNavigation.NavigateAsync<MyDeviceViewModel>(this);
        }

        protected override void OnBLEDevice_Connected(BLEDevice device)
        {
            base.OnBLEDevice_Connected(device);
        }

        /* Step 2 Create the new Device, set the Mac Address, update it, and then update the device name */
        protected override async void BLECharacteristicRead(BLECharacteristicsValue characteristic)
        {
            base.BLECharacteristicRead(characteristic);

            if (characteristic.Uid == NuvIoTGATTProfile.CHAR_UUID_SYS_CONFIG)
            {
                var parts = characteristic.Value.Split(',');
                var deviceTypeId = parts[3];
                var result = await DeviceManagementClient.CreateNewDeviceAsync(AppConfig.DeviceRepoId, deviceTypeId);
                if (result.Successful)
                {
                    CurrentDevice = result.Model;
                    CurrentDevice.MacAddress = SelecctedDevice.DeviceAddress;
                    await DeviceManagementClient.UpdateDeviceAsync(AppConfig.DeviceRepoId, result.Model);
                    await ViewModelNavigation.NavigateAsync<MyDeviceMenuViewModel>(this, DeviceLaunchArgsParam);
                }
            }
        }

        public async void ConnectAsync(BLEDevice device)
        {
            await GattConnection.ConnectAsync(device);
        }

        BLEDevice _selecctedDevice;
        public BLEDevice SelecctedDevice 
        { 
            get { return _selecctedDevice; }
            set
            {
                Set(ref _selecctedDevice, value);
                ConnectAsync(value);
            }
        }
    }
}
