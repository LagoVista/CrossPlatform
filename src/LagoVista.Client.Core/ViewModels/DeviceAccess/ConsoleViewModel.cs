using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class ConsoleViewModel : AppViewModelBase
    {
        public const string DeviceId = "DEVICE_ID";
        public const string DeviceRepoId = "DEVICE_REPO_ID";

        private String _deviceRepoId;
        private String _deviceId;
        private int _sendIndex;

        private BTDevice _currentDevice;
        private IBluetoothSerial _btSerial;

        public ConsoleViewModel()
        {
            _btSerial = SLWIOC.Create<IBluetoothSerial>();
            _btSerial.DeviceConnected += _btSerial_DeviceConnected;
            _btSerial.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;
        }

        private void _btSerial_DeviceDisconnected(object sender, BTDevice e)
        {
            Lines.Insert(0, "Device Disconnected");
        }

        private void _btSerial_DeviceConnected(object sender, BTDevice e)
        {

        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            IsBusy = true;

            _deviceRepoId = LaunchArgs.Parameters[ConsoleViewModel.DeviceRepoId].ToString();
            _deviceId = LaunchArgs.Parameters[ConsoleViewModel.DeviceId].ToString();


            if (!await Storage.HasKVPAsync(PairBTDeviceViewModel.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId)))
            {
                await Popups.ShowAsync("Must associate a BT Device first.");
                IsBusy = false;
                return;
            }

            var devices = await _btSerial.SearchAsync();
            var btDeviceId = await Storage.GetKVPAsync<string>(PairBTDeviceViewModel.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId));

            var btDevice = devices.Where(bt => bt.DeviceId == btDeviceId).First();
            if (btDevice == null)
            {
                await Popups.ShowAsync("Could not find paired device.");
                IsBusy = false;
                return;
            }

            try
            {
                Lines.Insert(0, "connecting to device.");
                await _btSerial.ConnectAsync(btDevice);
                _currentDevice = btDevice;
                Lines.Insert(0, "device connected.");
                IsBusy = false;
            }
            catch (Exception)
            {
                IsBusy = false;
                await Popups.ShowAsync("Could not connect to device.");
            }
        }


        StringBuilder _builder = new StringBuilder();
        private async void _btSerial_ReceivedLine(object sender, string e)
        {
            var lines = e.Split('\n');
            foreach (var line in lines)
            {
                var cleanLine = line.Trim();
                if (!String.IsNullOrEmpty(cleanLine))
                    Lines.Insert(0, cleanLine);
            }
        }

        public ObservableCollection<string> Lines { get; } = new ObservableCollection<string>();
        public override async Task IsClosingAsync()
        {
            if (_currentDevice != null)
            {
                // set to temporary value so we can check that it's no
                // longer current and not popup a device disconnected
                // message
                var device = _currentDevice;
                _currentDevice = null;
                try
                {
                    await _btSerial.DisconnectAsync(device);
                }
                catch (Exception) { }
            }

            await base.IsClosingAsync();
        }
    }
}
