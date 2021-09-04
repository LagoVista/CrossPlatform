using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace LagoVista.Core.UWP.Services
{
    public class GattConnection : IGATTConnection
    {
        private readonly IDispatcherServices _dispatcherService;
        private readonly List<BLEService> _knownServices = new List<BLEService>();
        private readonly BluetoothLEAdvertisementWatcher _watcher;
        private readonly List<GattCharacteristic> _subscribedCharacteristics = new List<GattCharacteristic>();
        private readonly List<BluetoothLEDevice> _windowsBLEDevices = new List<BluetoothLEDevice>();


        public GattConnection(IDispatcherServices dispatcherServices)
        {
            _watcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            _watcher.Received += Watcher_Received;
            _dispatcherService = dispatcherServices ?? throw new ArgumentNullException(nameof(dispatcherServices));
        }

        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var device = new BLEDevice()
            {
                DeviceAddress = args.BluetoothAddress.ToMacAddress(),
                DeviceName = args.Advertisement.LocalName,
            };

            Debug.WriteLine(device.DeviceName);

            if (device.DeviceName.Contains("NuvIoT"))
            {
                _dispatcherService.Invoke(() =>
                {
                    lock (DiscoveredDevices)
                    {
                        var existingDevice = DiscoveredDevices.Where(dev => dev.DeviceAddress == device.DeviceAddress).FirstOrDefault();
                        if (existingDevice != null)
                        {
                            existingDevice.LastSeen = DateTime.Now;
                        }
                        else
                        {
                            device.LastSeen = DateTime.Now;
                            DiscoveredDevices.Add(device);
                        }

                        DeviceDiscovered?.Invoke(this, device);
                    }
                });
            }
        }

        public bool IsScanning => throw new NotImplementedException();

        public ObservableCollection<BLEDevice> DiscoveredDevices { get; } = new ObservableCollection<BLEDevice>();

        public ObservableCollection<BLEDevice> ConnectedDevices { get; } = new ObservableCollection<BLEDevice>();

        public event EventHandler<BLEDevice> DeviceDiscovered;
        public event EventHandler<BLEDevice> DeviceConnected;
        public event EventHandler<BLEDevice> DeviceDisconnected;
        public event EventHandler<BLECharacteristicsValue> CharacteristicChanged;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler<string> DFUFailed;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> ReceiveConsoleOut;

        public async Task ConnectAsync(BLEDevice device)
        {
            var winBleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(device.DeviceAddress.FromMacAddress());
            _windowsBLEDevices.Add(winBleDevice);
            winBleDevice.ConnectionStatusChanged += WinBleDevice_ConnectionStatusChanged;

            var gatt = await winBleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            await Task.Delay(150);
            winBleDevice.DeviceInformation.Pairing.Custom.PairingRequested += Custom_PairingRequested;
            var pairingResult = await winBleDevice.DeviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly);
        }
        private void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            args.Accept();
        }

        private async void WinBleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var bleDevice = DiscoveredDevices.First(device => device.DeviceAddress == sender.BluetoothAddress.ToMacAddress());

            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                _windowsBLEDevices.Add(sender);
                _dispatcherService.Invoke(() =>
                {
                    DeviceConnected?.Invoke(this, bleDevice);
                    ConnectedDevices.Add(bleDevice);
                });
            }
            else
            {
                await DisconnectAsync(bleDevice);
            }
        }

        public async Task DisconnectAsync(BLEDevice device)
        {
            var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
            if (winDevice != null)
            {
                foreach (var characteristics in _subscribedCharacteristics)
                {
                    await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    characteristics.ValueChanged -= Characteristic_ValueChanged;
                }

                _subscribedCharacteristics.Clear();

                _windowsBLEDevices.Remove(winDevice);

                winDevice.Dispose();
                winDevice = null;
            }

            _dispatcherService.Invoke(() =>
            {
                ConnectedDevices.Remove(device);
                DeviceDisconnected?.Invoke(this, device);
            });
        }

        public async Task<byte[]> ReadCharacteristicAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
            var services = await winDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            var gattService = services.Services.First(svc => svc.Uuid.ToString() == service.Id);
            var characteristics = await gattService.GetCharacteristicsAsync();
            var gattCharacteristic = characteristics.Characteristics.First(chr => chr.Uuid.ToString() == characteristic.Id);
            var readResult = await gattCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

            var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(readResult.Value);
            var buffer = dataReader.ReadBuffer(readResult.Value.Length);
            return buffer.ToArray();
        }

        public void RegisterKnownServices(IEnumerable<BLEService> services)
        {
            _knownServices.AddRange(services);
        }

        public Task StartScanAsync()
        {
            _watcher.Start();

            return Task.CompletedTask;
        }

        public Task StopScanAsync()
        {
            _watcher.Stop();

            return Task.CompletedTask;
        }

        private void Characteristic_ValueChanged(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic sender, Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs args)
        {

        }

        public async Task<bool> SubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
            var services = await winDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
            var gattService = services.Services.First(svc => svc.Uuid.ToString() == service.Id);
            var characteristics = await gattService.GetCharacteristicsAsync();
            var gattCharacteristic = characteristics.Characteristics.First(chr => chr.Uuid.ToString() == characteristic.Id);
            GattCommunicationStatus statusResult = await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            gattCharacteristic.ValueChanged += Characteristic_ValueChanged;
            return statusResult == GattCommunicationStatus.Success;
        }

        public async Task<bool> UnsubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            var gattCharacteristic = _subscribedCharacteristics.Find(chr => chr.Uuid.ToString() == characteristic.Id);
            await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            gattCharacteristic.ValueChanged -= Characteristic_ValueChanged;

            _subscribedCharacteristics.Remove(gattCharacteristic);

            return true;
        }

        public Task<bool> UpdateCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task.FromResult<bool>(true);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, string str)
        {
            return Task.FromResult<bool>(true);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, byte[] str)
        {
            return Task.FromResult<bool>(true);
        }
    }

    public static class BLEAddressHelper
    {
        public static string ToMacAddress(this ulong addr)
        {
            var bldr = new StringBuilder();
            for (var idx = 0; idx < 6; ++idx)
            {
                var ch = addr & 0xFF;
                bldr.Insert(0, $"{ch:x2}:");
                addr = addr >> 8;
            }

            return bldr.ToString().TrimEnd(':').ToUpper();
        }

        public static ulong FromMacAddress(this string macAddress)
        {
            ulong addr = 0;
            var parts = macAddress.Split(':');
            for (var idx = 0; idx < 6; ++idx)
            {

                var hex = Convert.ToUInt32(parts[idx].Trim(':'), 16);
                addr += hex;
                if (idx < 5)
                {
                    addr = addr << 8;
                }
            }

            return addr;
        }
    }

}
