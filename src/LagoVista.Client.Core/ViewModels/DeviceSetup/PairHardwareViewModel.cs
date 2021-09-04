using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceSetup
{
    public class PairHardwareViewModel : DeviceViewModelBase
    {
        public PairHardwareViewModel()
        {
            DiscoveredDevices = GattConnection.DiscoveredDevices;
        }

        public ObservableCollection<BLEDevice> DiscoveredDevices { get; private set; }

        public async override Task InitAsync()
        {
            await GattConnection.StartScanAsync();
            GattConnection.DeviceConnected += GattConnection_DeviceConnected;
            await base.InitAsync();
        }

        private async void GattConnection_DeviceConnected(object sender, BLEDevice device)
        {
            base.OnBLEDevice_Connected(device);
            var service = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Find(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.Find(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_SYS_CONFIG);
            var result = await GattConnection.ReadCharacteristicAsync(device, service, characteristics);
            var str = System.Text.ASCIIEncoding.ASCII.GetString(result);
            var parts = str.Split(',');
            var deviceModelId = parts[1];
            if(deviceModelId.Length == 32)
            {
                await PerformNetworkOperation(async () =>
                {
                    var existingDevice = await DeviceManagementClient.GetDeviceByMacAddressAsync(AppConfig.DeviceRepoId, device.DeviceAddress);
                    var nuvIoTDevice = await DeviceManagementClient.CreateNewDeviceAsync(AppConfig.DeviceRepoId, deviceModelId);
                });
            }
            
        }

        protected override void OnBLEDevice_Connected(BLEDevice device)
        {
            
        }

        public async override Task IsClosingAsync()
        {
            GattConnection.DeviceConnected += GattConnection_DeviceConnected;
            await GattConnection.StopScanAsync();
            await base.IsClosingAsync();
        }

        /* Step 2 Create the new Device, set the Mac Address, update it, and then update the device name */
        protected override async void BLECharacteristicRead(BLECharacteristicsValue characteristic)
        {
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
