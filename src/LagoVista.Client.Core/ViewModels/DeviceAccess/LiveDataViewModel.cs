using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.IOC;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class LiveDataViewModel : DeviceViewModelBase
    {
        private IBluetoothSerial _btSerial;
        
        IOConfig _config;

        public LiveDataViewModel()
        {
            _btSerial = SLWIOC.Get<IBluetoothSerial>();
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;
            _btSerial.DeviceDisconnected += _btSerial_DeviceDisconnected;
        }

        private async void _btSerial_DeviceDisconnected(object sender, BTDevice e)
        {
                await Popups.ShowAsync("Device Disconnected.");
        }

        private void _btSerial_ReceivedLine(object sender, string e)
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
            _config = await Storage.GetAsync<IOConfig>($"{DeviceId}.ioconfig.json");

            await base.InitAsync();

            if (!await Storage.HasKVPAsync(PairBTDeviceViewModel.ResolveBTDeviceIdKey(DeviceRepoId, DeviceId)))
            {
                await Popups.ShowAsync("Must associate a BT Device first.");
                return;
            }

            var devices = await _btSerial.SearchAsync();
            var btDeviceId = await Storage.GetKVPAsync<string>(PairBTDeviceViewModel.ResolveBTDeviceIdKey(DeviceRepoId, DeviceId));

            var btDevice = devices.Where(bt => bt.DeviceId == btDeviceId).First();
            if (btDevice == null)
            {
                await Popups.ShowAsync("Could not find paired device.");
                return;
            }
        }

        public ObservableCollection<LiveDataItem> DataItems { get; set; } = new ObservableCollection<LiveDataItem>();
    }
}
