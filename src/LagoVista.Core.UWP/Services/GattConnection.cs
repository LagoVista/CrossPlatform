using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace LagoVista.Core.UWP.Services
{
    public class GattConnection : IGATTConnection
    {
        public bool IsScanning { get; private set; }

        public ObservableCollection<BLEDevice> DiscoveredDevices { get; } = new ObservableCollection<BLEDevice>();

        public ObservableCollection<BLEDevice> ConnectedDevices { get; } = new ObservableCollection<BLEDevice>();

        private readonly List<BLEDevice> _internalConnectedDevices = new List<BLEDevice>();

        public event EventHandler<BLEDevice> DeviceDiscovered;
        public event EventHandler<BLEDevice> DeviceConnected;
        public event EventHandler<BLEDevice> DeviceDisconnected;
        public event EventHandler<BLECharacteristicsValue> CharacteristicChanged;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler<string> DFUFailed;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> ReceiveConsoleOut;

        public Dictionary<BLEDevice, GattCharacteristic> _pingCharacteristics = new Dictionary<BLEDevice, GattCharacteristic>();

        private readonly IDispatcherServices _dispatcherService;
        private readonly List<BLEService> _knownServices = new List<BLEService>();
        private readonly BluetoothLEAdvertisementWatcher _watcher;
        private readonly ObservableCollection<BLEDevice> _connectingDevices = new ObservableCollection<BLEDevice>();
        private readonly List<GattCharacteristic> _subscribedCharacteristics = new List<GattCharacteristic>();

        private readonly Dictionary<BLEDevice, List<GattCharacteristic>> _allCharacteristics = new Dictionary<BLEDevice, List<GattCharacteristic>>();
        private readonly Dictionary<BLEDevice, List<GattDeviceService>> _allDeviceServices = new Dictionary<BLEDevice, List<GattDeviceService>>();

        private readonly List<BluetoothLEDevice> _windowsBLEDevices = new List<BluetoothLEDevice>();
        private readonly Timer _watchdogTimer;

        private readonly SemaphoreSlim _deviceAccessLocker = new SemaphoreSlim(1, 1);

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
            await _deviceAccessLocker.WaitAsync();
            var start = DateTime.Now;
            Debug.WriteLine($"=> Tick - Start connected devices {_internalConnectedDevices.Count}, connecting Devices {_connectingDevices.Count}");
            try
            {
                var devicesToRemove = new List<BLEDevice>();

                foreach (var connectedDevice in _internalConnectedDevices)
                {
                    if ((DateTime.Now - connectedDevice.LastSeen).TotalSeconds > 3)
                    {
                        devicesToRemove.Add(connectedDevice);
                    }
                }

                foreach (var device in devicesToRemove)
                {
                    Debug.WriteLine($"====> Tick - Timeout, will remove device {device.DeviceName}");
                    await PrivateDisconnectAsync(device);
                }

                devicesToRemove.Clear();

                foreach (var connectedDevice in _internalConnectedDevices.ToList())
                {
                    if (connectedDevice.Connected)
                    {
                        var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == connectedDevice.DeviceAddress);
                        if (winDevice != null && winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                        {
                            try
                            {
                                if (_pingCharacteristics.ContainsKey(connectedDevice))
                                {
                                    var pingCharacteristics = _pingCharacteristics[connectedDevice];
                                    var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes("ping");
                                    GattCommunicationStatus statusResult = await pingCharacteristics.WriteValueAsync(buffer.AsBuffer());
                                    var success = statusResult == GattCommunicationStatus.Success;
                                    Debug.WriteLine($"====> Tick - Send Ping: {connectedDevice.DeviceName}");
                                }
                                else
                                {
                                    Debug.WriteLine($"!===> Tick - could not find subscription to send ping -{connectedDevice.DeviceName}");
                                }
                            }
                            catch
                            {
                                Debug.WriteLine($"!===> Tick - exception sending ping to - {connectedDevice.DeviceName}");
                                await PrivateDisconnectAsync(connectedDevice);
                            }
                        }
                    }
                }

                foreach (var device in _connectingDevices)
                {
                    if (!device.ConnectingTimeStamp.HasValue)
                    {
                        Debug.WriteLine($"====> Tick - Connecting device does not have time stamp, removing: {device.DeviceName}");
                        devicesToRemove.Add(device);
                    }
                    else
                    {
                        var delta = DateTime.Now - device.ConnectingTimeStamp.Value;
                        if (delta.TotalSeconds > 5)
                        {
                            Debug.WriteLine($"====> Tick - Connecting Timeout, will remove connecting device {device.DeviceName}");
                            devicesToRemove.Add(device);
                        }
                    }
                }

                foreach (var device in devicesToRemove)
                {
                    _connectingDevices.Remove(device);
                }
            }
            finally
            {
                _deviceAccessLocker.Release();
                Debug.WriteLine($"=> Tick - Finish connected devices {_internalConnectedDevices.Count}, connecting Devices {_connectingDevices.Count}");
            }
        }

        private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            await _deviceAccessLocker.WaitAsync();
            try
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
                    });
                }
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        public async Task ConnectAsync(BLEDevice device)
        {
            await _deviceAccessLocker.WaitAsync();

            try
            {
                Debug.WriteLine($"=> Connect - Start {device.DeviceName}");
                if (_connectingDevices.Any(cdvc => cdvc.DeviceAddress == device.DeviceAddress))
                {
                    _deviceAccessLocker.Release();
                    Debug.WriteLine($"=> Connect - Already Connecting, aborting {device.DeviceName}");
                    return;
                }

                device.ConnectingTimeStamp = DateTime.Now;
                _connectingDevices.Add(device);

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
                else
                {
                    Debug.WriteLine($"===> Pairing Result {pairingResult.Status}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!=> Connect - Exception {device.DeviceName} - {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                await PrivateDisconnectAsync(device);
            }
            finally
            {
                _deviceAccessLocker.Release();
                Debug.WriteLine($"=> Connect - End {device.DeviceName}");
            }
        }

        private void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            args.Accept();
        }

        #region Device Connection Status Changed
        private async void WinBleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            await _deviceAccessLocker.WaitAsync();
            Debug.WriteLine($"=> Connect Status Changed - Start");

            try
            {
                var bleDevice = DiscoveredDevices.FirstOrDefault(device => device.DeviceAddress == sender.BluetoothAddress.ToMacAddress());
                if (bleDevice == null)
                {
                    Debug.WriteLine($"!==> Could not find discovered device with Mac Address {sender.BluetoothAddress.ToMacAddress()}");
                    _deviceAccessLocker.Release();
                    Debug.WriteLine($"==> Connect Status Changed - Finish");
                    return;
                }

                Debug.WriteLine($"==> Connect Status Changed - {bleDevice.DeviceName} - {sender.ConnectionStatus}");

                if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    var nuviotService = NuvIoTGATTProfile.GetNuvIoTGATT().Services.FirstOrDefault(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
                    var stateCharacteristics = nuviotService.Characteristics.FirstOrDefault(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_STATE);

                    var services = await sender.GetGattServicesAsync(BluetoothCacheMode.Cached);
                    if (services.Status != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine($"!==> Could not get services - Status {services.Status} ");
                        return;
                    }

                    var gattService = services.Services.FirstOrDefault(svc => svc.Uuid.ToString() == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
                    if (gattService == null)
                    {
                        Debug.WriteLine($"!==> Could not find service - {nuviotService.Name} - {nuviotService.Id} - Services Found {services.Services.Count}. ");
                        foreach (var deviceService in services.Services)
                        {
                            Debug.WriteLine($"   Found Service - {deviceService.Uuid}");
                        }
                        _deviceAccessLocker.Release();
                        return;
                    }

                    if (_allDeviceServices.ContainsKey(bleDevice))
                    {
                        _allDeviceServices.Remove(bleDevice);
                    }

                    _allDeviceServices.Add(bleDevice, services.Services.ToList());

                    Debug.WriteLine($"==> Subscribe - Found Service  {NuvIoTGATTProfile.SVC_UUID_NUVIOT} - {gattService.Uuid}.");

                    var characteristics = await gattService.GetCharacteristicsAsync();

                    if (_allCharacteristics.ContainsKey(bleDevice))
                    {
                        _allCharacteristics.Remove(bleDevice);
                    }

                    if (characteristics.Status != GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine($"!==> Could not get characteristics - Status {characteristics.Status} - for service");
                        return;
                    }

                    var gattCharacteristic = characteristics.Characteristics.FirstOrDefault(chr => chr.Uuid.ToString() == NuvIoTGATTProfile.CHAR_UUID_STATE);
                    if (gattCharacteristic == null)
                    {
                        Debug.WriteLine($"!==> Could not find characteristic - {stateCharacteristics.Name} = {stateCharacteristics.Id} - Characteristic found {characteristics.Characteristics.Count}");
                        foreach (var deviceCharacteristic in characteristics.Characteristics)
                        {
                            Debug.WriteLine($"   Found Characteristic - {deviceCharacteristic.Uuid}");
                        }

                        return;
                    }

                    Debug.WriteLine($"===> Connect - Added Subscription");

                    var idx = 1;
                    _allCharacteristics.Add(bleDevice, characteristics.Characteristics.ToList());
                    foreach (var charactersistc in characteristics.Characteristics)
                    {
                        Debug.WriteLine($"====> {idx++}. Service Characteristics {charactersistc.Uuid} ");
                    }                    

                    _pingCharacteristics.Add(bleDevice, gattCharacteristic);

                    if (_connectingDevices.Contains(bleDevice))
                        _connectingDevices.Remove(bleDevice);

                    bleDevice.Connected = true;
                    bleDevice.LastSeen = DateTime.Now;
                    _watchdogTimer.Start();
                    _windowsBLEDevices.Add(sender);
                    _internalConnectedDevices.Add(bleDevice);
                    Debug.WriteLine($"===> Connect Status Changed - Adding _internalConnectedDevices Device: {bleDevice.DeviceName}");

                    _dispatcherService.Invoke(() =>
                    {
                        DeviceConnected?.Invoke(this, bleDevice);
                        ConnectedDevices.Add(bleDevice);
                    });
                }
                else
                {
                    Debug.WriteLine($"===> Connect Status Changed - removing _Connecting devices: {bleDevice.DeviceName}");

                    if (_connectingDevices.Contains(bleDevice))
                        _connectingDevices.Remove(bleDevice);

                    await PrivateDisconnectAsync(bleDevice);
                }
            }
            finally
            {
                _deviceAccessLocker.Release();
                Debug.WriteLine($"=> Connect Status Changed - Finish");
            }
        }
        #endregion

        public async Task DisconnectAsync(BLEDevice device)
        {
            await _deviceAccessLocker.WaitAsync();

            try
            {
                await PrivateDisconnectAsync(device);
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        #region Disconnect
        public async Task PrivateDisconnectAsync(BLEDevice device)
        {
            if (_deviceAccessLocker.CurrentCount > 0)
            {
                throw new InvalidOperationException("Attempt to disconnect while not in access locker.");
            }

            if (_pingCharacteristics.ContainsKey(device))
            {
                _pingCharacteristics.Remove(device);
            }

            device.Connected = false;

            Debug.WriteLine($"====> Disconnect - Start: {device.DeviceName}");


            _subscribedCharacteristics.Clear();

            _allCharacteristics.Remove(device);

            var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
            if (winDevice != null)
            {
                winDevice.ConnectionStatusChanged -= WinBleDevice_ConnectionStatusChanged;

                if (winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    Debug.WriteLine($"======> Disconnect - Unsubscribe notifications : {_subscribedCharacteristics.Count}");
                    foreach (var characteristic in _subscribedCharacteristics)
                    {
                        characteristic.ValueChanged -= Characteristic_ValueChanged;
                        try
                        {
                            await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                            Debug.WriteLine($"========> Disconnect - Unsubscribe notification {characteristic.Uuid}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"!! Exception in WriteCharacteristic to disconnect: {ex.Message} {characteristic.Uuid}");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"======> Disconnect - Device not connected, can not unsubscribe.");
                }

                Debug.WriteLine($"======> Disconnect - Unsubscribed");

                try
                {
                    foreach (var srvc in _allDeviceServices[device])
                    {
                        try
                        {
                            srvc.Dispose();
                            Debug.WriteLine($"======> Disconnect - Disposed Service {srvc.Uuid}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"!! Exception in srvc.Dispose() to disconnect: {ex.Message} !!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"!! Exception GetGattServicesAsync: {ex.Message} !!");
                }

                _windowsBLEDevices.Remove(winDevice);

                Debug.WriteLine($"======> Disconnect - Removed BLE Device");

                try
                {
                    winDevice.Dispose();
                    Debug.WriteLine($"======> Disconnect - Dispose BLE Device");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"!! Exception in winDevice.Dispose() to disconnect: {ex.Message} !!");
                }
                winDevice = null;
            }
            else
            {
                Debug.WriteLine($"======> Disconnect - No BLE device to remove");
            }

            if (_internalConnectedDevices.Contains(device))
            {
                _internalConnectedDevices.Remove(device);
                Debug.WriteLine($"======> Disconnect - Remove internal connected");
                _dispatcherService.Invoke(() =>
                {
                    ConnectedDevices.Remove(device);
                    DeviceDisconnected?.Invoke(this, device);
                });
            }
            else
            {
                Debug.WriteLine($"======> Disconnect - No internal connected to remove");
                Debug.WriteLine($"No connected devices to disconnect: {device.DeviceName}");
            }

            Debug.WriteLine($"====> Disconnect - End");
        }
        #endregion

        #region Read Characteristic 
        public async Task<byte[]> ReadCharacteristicAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            await _deviceAccessLocker.WaitAsync();

            try
            {
                var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
                if (winDevice != null && winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
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
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ReadCharacteristicAsync: {ex.Message}");
                await PrivateDisconnectAsync(device);
                return null;
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }
        #endregion

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
            });
        }

        #region Subscribe
        public async Task<bool> SubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {

            await _deviceAccessLocker.WaitAsync();

            Debug.WriteLine($"==> Subscribe Start - {characteristic.Id}");

            try
            {
                var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
                if (winDevice != null && winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    if (_allCharacteristics.ContainsKey(device))
                    {
                        var gattCharacteristic = _allCharacteristics[device].FirstOrDefault(chr => chr.Uuid.ToString() == characteristic.Id);
                        if (gattCharacteristic != null)
                        {
                            Debug.WriteLine($"====> Subscribe - Found Characteristic {characteristic.Name}.");

                            GattCommunicationStatus statusResult = await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            if (statusResult == GattCommunicationStatus.Success)
                            {
                                gattCharacteristic.ValueChanged += Characteristic_ValueChanged;
                                _subscribedCharacteristics.Add(gattCharacteristic);
                                Debug.WriteLine($"====> Subscribe - Subscribed to characteristic {characteristic.Name}.");
                            }
                            else
                            {
                                Debug.WriteLine($"====> Subscribe - could not subscribe to characteristic {characteristic.Name} {statusResult}.");
                            }

                            return statusResult == GattCommunicationStatus.Success;
                        }
                        else
                        {
                            Debug.WriteLine($"!==> Could not find characteristic - {characteristic.Name} = {characteristic.Id}.");
                        }
                    }

                    return false;
                }
                else
                {
                    Debug.WriteLine($"!==> Subscribe - Could not find BLE Device with {device.DeviceAddress}");
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!====> Exception in SubscribeAsync: {ex.Message}");
                await PrivateDisconnectAsync(device);
                return false;
            }
            finally
            {
                _deviceAccessLocker.Release();
                Debug.WriteLine($"==> Subscribe Completed - {characteristic.Id}");
            }
        }
        #endregion

        #region Subscribe
        public async Task<bool> UnsubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            await _deviceAccessLocker.WaitAsync();

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
                await PrivateDisconnectAsync(device);
                return false;
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }
        #endregion

        public Task<bool> UpdateCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task.FromResult<bool>(true);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, string str)
        {
            return WriteCharacteristic(device, service, characteristic, System.Text.ASCIIEncoding.ASCII.GetBytes(str));
        }

        public async Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, byte[] buffer)
        {
            await _deviceAccessLocker.WaitAsync();

            try
            {
                var winDevice = _windowsBLEDevices.FirstOrDefault(dvc => dvc.BluetoothAddress.ToMacAddress() == device.DeviceAddress);
                if (winDevice != null && winDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                {
                    var services = await winDevice.GetGattServicesAsync(BluetoothCacheMode.Cached);
                    var gattService = services.Services.First(svc => svc.Uuid.ToString() == service.Id);
                    var characteristics = await gattService.GetCharacteristicsAsync();
                    var gattCharacteristic = characteristics.Characteristics.First(chr => chr.Uuid.ToString() == characteristic.Id);

                    GattCommunicationStatus statusResult = await gattCharacteristic.WriteValueAsync(buffer.AsBuffer());
                    return statusResult == GattCommunicationStatus.Success;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in WriteCharacteristic: {ex.Message}");
                await PrivateDisconnectAsync(device);
                return false;
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }
    }

    #region Address Helper
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
    #endregion

}
