using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class BTDevicePickerViewModel : AppViewModelBase
    {
        private IBluetoothSerial _btSerial;

        public BTDevicePickerViewModel()
        {
            _btSerial = SLWIOC.Create<IBluetoothSerial>();
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            var devices = await _btSerial.SearchAsync();
            ConnectedDevices.Clear();
            foreach (var device in devices)
            {
                if (device.DeviceName.StartsWith("NuvIoT"))
                {
                    ConnectedDevices.Add(device);
                }
            }
        }

        private BTDevice _selectedDevice;
        public BTDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set { ConnectDeviceAsync(value); }
        }

        private async void ConnectDeviceAsync(BTDevice device)
        {
            try
            {
                await _btSerial.ConnectAsync(device);

                Set(ref _selectedDevice, device);

//                var updateResult = await SetBTDeviceIdAsync();
                //if (!updateResult.Successful)
                {
                  //  throw new Exception(updateResult.Errors.First().Message);
                }

  //              await Storage.StoreKVP(App.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId), device.DeviceId);
                await _btSerial.DisconnectAsync(device);

                CloseScreen();
            }
            catch (Exception)
            {
                await _btSerial.DisconnectAsync(device);
                await Popups.ShowAsync($"Could not connect to {device.DeviceName}");
            }
        }

        public ObservableCollection<BTDevice> ConnectedDevices { get; private set; } = new ObservableCollection<BTDevice>();

        public RelayCommand PairDeviceCommand { get; private set; }
    }
}
