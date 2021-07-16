using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.IOC;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SeaWolf.ViewModels
{
    public class LiveDataViewModel : AppViewModelBase
    {
        private readonly IBluetoothSerial _btSerial;
        private String _deviceRepoId;
        private String _deviceId;

        IOConfig _config;

        public LiveDataViewModel()
        {
            _btSerial = SLWIOC.Create<IBluetoothSerial>();
            _btSerial.ReceivedLine += BtSerial_ReceivedLine;
        }

        private void BtSerial_ReceivedLine(object sender, string e)
        {
            DispatcherServices.Invoke(() =>
            {
                var lines = e.Split('\r');
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        var existingItem = DataItems.FirstOrDefault(lst => lst.Key == key);
                        if (existingItem != null)
                        {
                            existingItem.Value = value;
                        }
                        else
                        {
                            var label = key;
                            if (_config != null)
                            {
                                label = _config.ResolveLabel(key);
                            }

                            DataItems.Add(new LiveDataItem(key, label, value));
                        }
                    }

                }
            });
        }

        public override async Task InitAsync()
        {
            _deviceRepoId = LaunchArgs.Parameters[ComponentViewModel.DeviceRepoId].ToString();
            _deviceId = LaunchArgs.Parameters[ComponentViewModel.DeviceId].ToString();
            _config = await Storage.GetAsync<IOConfig>($"{_deviceId}.ioconfig.json");

            await base.InitAsync();

            if (!await Storage.HasKVPAsync(App.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId)))
            {
                await Popups.ShowAsync("Must associate a BT Device first.");
                return;
            }

            var devices = await _btSerial.SearchAsync();
            var btDeviceId = await Storage.GetKVPAsync<string>(App.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId));

            var btDevice = devices.Where(bt => bt.DeviceId == btDeviceId).First();
            if (btDevice == null)
            {
                await Popups.ShowAsync("Could not find paired device.");
                return;
            }

            await _btSerial.ConnectAsync(btDevice);
        }

        public ObservableCollection<LiveDataItem> DataItems { get; set; } = new ObservableCollection<LiveDataItem>();

        public override async Task IsClosingAsync()
        {

            await base.IsClosingAsync();
        }
    }
}
