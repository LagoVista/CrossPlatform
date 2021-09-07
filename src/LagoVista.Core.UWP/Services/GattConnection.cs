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
        private readonly ObservableCollection<BLEDevice> _connectingDevices = new ObservableCollection<BLEDevice>();
        private readonly List<GattCharacteristic> _subscribedCharacteristics = new List<GattCharacteristic>();
        private readonly List<BluetoothLEDevice> _windowsBLEDevices = new List<BluetoothLEDevice>();
        private readonly Timer _watchdogTimer;

        private static int _instanceCount = 0;

        public GattConnection(IDispatcherServices dispatcherServices)
        {
            _watcher = new BluetoothLEAdvertisementWatcher()
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            _watchdogTimer = new Timer
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            _watchdogTimer.Tick += WatchdogTimer_Tick;
            _watchdogTimer.Start();

            _watcher.Received += Watcher_Received;
            _dispatcherService = dispatcherServices ?? throw new ArgumentNullException(nameof(dispatcherServices));

            if (_instanceCount > 0)
            {
                throw new InvalidOperationException("Attempt to create more then one GattConnection.");
            }

            _instanceCount++;
        }

        private async void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            var devicesToRemove = new List<BLEDevice>();

            lock (ConnectedDevices)
            {
                foreach (var connectedDevice in ConnectedDevices)
                {
                    if ((DateTime.Now - connectedDevice.LastSeen).TotalSeconds > 3)
                    {
                        devicesToRemove.Add(connectedDevice);
                    }
                }
            }

            foreach (var device in devicesToRemove)
            {
                await DisconnectAsync(device);
            }

            devicesToRemove.Clear();

            lock (_connectingDevices)
            {
                foreach (var device in _connectingDevices)
                {
                    if (!device.ConnectingTimeStamp.HasValue)
                    {
                        devicesToRemove.Add(device);
                    }
                    else
                    {
                        var delta = DateTime.Now - device.ConnectingTimeStamp.Value;
                        if (delta.TotalSeconds > 5)
                        {
                            devicesToRemove.Add(device);
                        }
                    }
                }

                foreach (var device in devicesToRemove)
                {
                    _connectingDevices.Remove(device);
                }

                if (devicesToRemove.Any())
                {
                    _watcher.Start();
                }
            }
        }

        private void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            var device = new BLEDevice()
            {
                DeviceAddress = args.BluetoothAddress.ToMacAddress(),
                DeviceName = args.Advertisement.LocalName,
            };

            Debug.WriteLine($"{device.DeviceAddress} - {device.DeviceName}");

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

        public bool IsScanning { get; private set; }

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
            _watcher.Stop();
            lock (_connectingDevices)
            {
                if (_connectingDevices.Contains(device))
                {
                    return;
                }

                _connectingDevices.Add(device);
            }

            try
            {
                var winBleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(device.DeviceAddress.FromMacAddress());
                _windowsBLEDevices.Add(winBleDevice);
                winBleDevice.ConnectionStatusChanged += WinBleDevice_ConnectionStatusChanged;

                var gatt = await winBleDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                await Task.Delay(150);
                winBleDevice.DeviceInformation.Pairing.Custom.PairingRequested += Custom_PairingRequested;
                var pairingResult = await winBleDevice.DeviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly);
                if (pairingResult.Status != DevicePairingResultStatus.Paired &&
                    pairingResult.Status != DevicePairingResultStatus.AlreadyPaired)
                {
                    throw new Exception($"Could not pair device, Status=> {pairingResult.Status}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!! Exception in Connect: {ex.Message} !!");
                await DisconnectAsync(device);
            }
        }

        private void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            args.Accept();
        }

        private async void WinBleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var bleDevice = DiscoveredDevices.First(device => device.DeviceAddress == sender.BluetoothAddress.ToMacAddress());

            Debug.WriteLine($"BLE Connection Status Changed: {bleDevice.DeviceName} - {sender.ConnectionStatus}");

            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                lock (_connectingDevices)
                {
                    _connectingDevices.Remove(bleDevice);
                }

                bleDevice.Connected = true;
                bleDevice.LastSeen = DateTime.Now;
                _watchdogTimer.Start();
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
            device.Connected = true;

            Debug.WriteLine($"Attempting to disconnect: {device.DeviceName}");

            foreach (var characteristics in _subscribedCharacteristics)
            {
                characteristics.ValueChanged -= Characteristic_ValueChanged;
                try
                {
                    await characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"!! Exception in WriteCharacteristic to disconnect: {ex.Message} !!");
                }
            }

            _subscribedCharacteristics.Clear();

            var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
            if (winDevice != null)
            {
                try
                {
                    var services = await winDevice.GetGattServicesAsync();

                    foreach (var srvc in services.Services)
                    {
                        srvc.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"!! Exception in GetGattServicesAsync to disconnect: {ex.Message} !!");
                }

                winDevice.ConnectionStatusChanged -= WinBleDevice_ConnectionStatusChanged;
                _windowsBLEDevices.Remove(winDevice);

                winDevice.Dispose();
                winDevice = null;

                Debug.WriteLine($"Removed BLE Device: {device.DeviceName}");
            }
            else
            {
                Debug.WriteLine($"No win ble devices to disconnect: {device.DeviceName}");
            }

            lock (ConnectedDevices)
            {
                if (ConnectedDevices.Contains(device))
                {
                    _dispatcherService.Invoke(() =>
                    {
                        ConnectedDevices.Remove(device);
                        DeviceDisconnected?.Invoke(this, device);
                    });
                }
                else
                {
                    _watcher.Start();
                    Debug.WriteLine($"No connected devices to disconnect: {device.DeviceName}");
                }
            }
        }

        public async Task<byte[]> ReadCharacteristicAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            try
            {
                var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
                var services = await winDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                var gattService = services.Services.First(svc => svc.Uuid.ToString() == service.Id);
                var characteristics = await gattService.GetCharacteristicsAsync();
                var gattCharacteristic = characteristics.Characteristics.First(chr => chr.Uuid.ToString() == characteristic.Id);
                var readResult = await gattCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

                using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(readResult.Value))
                {
                    var buffer = dataReader.ReadBuffer(readResult.Value.Length);
                    device.LastSeen = DateTime.Now;
                    return buffer.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ReadCharacteristicAsync: {ex.Message}");
                await DisconnectAsync(device);
                return null;
            }
        }

        public void RegisterKnownServices(IEnumerable<BLEService> services)
        {
            _knownServices.AddRange(services);
        }

        public Task StartScanAsync()
        {
            _watcher.Start();
            IsScanning = true;
            return Task.CompletedTask;
        }

        public Task StopScanAsync()
        {
            _watcher.Stop();
            IsScanning = false;
            return Task.CompletedTask;
        }

        private void Characteristic_ValueChanged(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic sender, Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs args)
        {
            _dispatcherService.Invoke(() =>
            {
                lock (ConnectedDevices)
                {
                    var firstDevice = ConnectedDevices.FirstOrDefault();
                    if (firstDevice != null)
                    {
                        firstDevice.LastSeen = DateTime.Now;
                    }

                    if (args.CharacteristicValue.Length > 0)
                    {
                        var buffer = args.CharacteristicValue.ToArray();
                        CharacteristicChanged?.Invoke(this, new BLECharacteristicsValue()
                        {
                            Uid = sender.Uuid.ToString(),
                            Value = System.Text.ASCIIEncoding.ASCII.GetString(args.CharacteristicValue.ToArray())
                        });
                    }
                }
            });
        }

        public async Task<bool> SubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            try
            {
                var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
                if (winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    var services = await winDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                    var gattService = services.Services.First(svc => svc.Uuid.ToString() == service.Id);
                    var characteristics = await gattService.GetCharacteristicsAsync();
                    var gattCharacteristic = characteristics.Characteristics.First(chr => chr.Uuid.ToString() == characteristic.Id);

                    GattCommunicationStatus statusResult = await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    gattCharacteristic.ValueChanged += Characteristic_ValueChanged;
                    return statusResult == GattCommunicationStatus.Success;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in SubscribeAsync: {ex.Message}");
                await DisconnectAsync(device);
                return false;
            }
        }

        public async Task<bool> UnsubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            try
            {
                var gattCharacteristic = _subscribedCharacteristics.Find(chr => chr.Uuid.ToString() == characteristic.Id);
                gattCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                _subscribedCharacteristics.Remove(gattCharacteristic);

                var result = await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                return result == GattCommunicationStatus.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in UnsubscribeAsync: {ex.Message}");
                await DisconnectAsync(device);
                return false;
            }
        }

        public Task<bool> UpdateCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task.FromResult<bool>(true);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, string str)
        {
            return WriteCharacteristic(device, service, characteristic, System.Text.ASCIIEncoding.ASCII.GetBytes(str));
        }

        public async Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, byte[] str)
        {
            try
            {
                var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
                if (winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    var services = await winDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                    var gattService = services.Services.First(svc => svc.Uuid.ToString() == service.Id);
                    var characteristics = await gattService.GetCharacteristicsAsync();
                    var gattCharacteristic = characteristics.Characteristics.First(chr => chr.Uuid.ToString() == characteristic.Id);

                    GattCommunicationStatus statusResult = await gattCharacteristic.WriteValueAsync(str.AsBuffer());
                    return statusResult == GattCommunicationStatus.Success;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in WriteCharacteristic: {ex.Message}");
                await DisconnectAsync(device);
                return false;
            }
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
                addr >>= 8;
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
                    addr <<= 8;
                }
            }

            return addr;
        }
    }

}
