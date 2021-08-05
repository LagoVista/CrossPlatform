using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using Java.Util;
using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Droid.Services
{

    public class GATTConnection : IGATTConnection
    {
        UUID NUVIOT_SRVC_UUID = UUID.FromString("d804b639-6ce7-4e80-9f8a-ce0f699085eb");
        UUID STATE_CHARACTERISTICS_UUID = UUID.FromString("d804b639-6ce7-5e81-9f8a-ce0f699085eb");


        public event EventHandler<BLEDevice> DeviceDiscovered;
        public event EventHandler<BLEDevice> DeviceConnected;
        public event EventHandler<BLEDevice> DeviceDisconnected;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler<string> DFUFailed;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> ReceiveConsoleOut;

        private CancellationTokenSource _listenCancelTokenSource = new CancellationTokenSource();
        private SemaphoreSlim _charactersiticRead = new SemaphoreSlim(0);
        private SemaphoreSlim _charactersiticWrote = new SemaphoreSlim(0);

        public bool IsScanning { get; private set; } = false;

        public class mScanCallback : ScanCallback
        {
            private readonly GATTConnection _connection;

            public mScanCallback(GATTConnection connection)
            {
                _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            }

            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                System.Diagnostics.Debug.WriteLine($"Device Name {result.Device.Name}");
                base.OnScanResult(callbackType, result);

                if (!System.String.IsNullOrEmpty(result.Device.Name))
                {
                    _connection.HandleDeviceDiscovered(result.Device);
                }
            }
        }

        public class mGattCallback : BluetoothGattCallback
        {
            private readonly GATTConnection _connection;

            public mGattCallback(GATTConnection connection)
            {
                _connection = connection ?? throw new ArgumentException(nameof(connection));
            }

            public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
            {
                base.OnConnectionStateChange(gatt, status, newState);

                if (newState == ProfileState.Connected)
                {
                    gatt.DiscoverServices();
                    _connection.HandleDeviceConnected(gatt);
                }

                if (newState == ProfileState.Disconnected)
                {
                    _connection.HandleDeviceDisconnected(gatt);
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
            {
                base.OnServicesDiscovered(gatt, status);
                _connection.HandleServicesDiscovered(gatt);
                foreach (var service in gatt.Services)
                {
                    Debug.WriteLine(service.Uuid);
                }
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
            {
                base.OnCharacteristicRead(gatt, characteristic, status);
                if (status == GattStatus.Success)
                {
                    _connection.CharacteristicsRead(characteristic.Uuid.ToString());
                }
            }

            public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
            {
                base.OnCharacteristicWrite(gatt, characteristic, status);
                if (status == GattStatus.Success)
                {
                    _connection.CharacteristicsWrote(characteristic.Uuid.ToString());
                }
            }

            public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
            {
                base.OnDescriptorWrite(gatt, descriptor, status);
            }

            public override void OnMtuChanged(BluetoothGatt gatt, int mtu, [GeneratedEnum] GattStatus status)
            {
                base.OnMtuChanged(gatt, mtu, status);
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                var value = characteristic.GetValue();

                var msg = System.Text.ASCIIEncoding.ASCII.GetString(value);

                Debug.WriteLine($"Characteristic Changed {characteristic.Uuid.ToString()} - {msg}");

                base.OnCharacteristicChanged(gatt, characteristic);
            }
        }

        private ObservableCollection<BLEDevice> _discoveredDevices = new ObservableCollection<BLEDevice>();
        private ObservableCollection<BLEDevice> _connectedDevices = new ObservableCollection<BLEDevice>();
        private ObservableCollection<BluetoothGatt> _bleDevices = new ObservableCollection<BluetoothGatt>();
        private BluetoothLeScanner _bluetoothLeScanner = BluetoothAdapter.DefaultAdapter.BluetoothLeScanner;

        private IDispatcherServices _dispatcherService;
        private readonly mScanCallback _scanCallbackHanndler;
        private readonly mGattCallback _gattCallback;

        readonly List<BLEService> _knownServices = new List<BLEService>();

        private readonly List<BluetoothDevice> _androidDevices = new List<BluetoothDevice>();

        public GATTConnection(IDispatcherServices dispatcherServices)
        {
            _scanCallbackHanndler = new mScanCallback(this);
            _gattCallback = new mGattCallback(this);
            _dispatcherService = dispatcherServices ?? throw new ArgumentNullException(nameof(dispatcherServices));
        }

        public void RegisterKnownServices(IEnumerable<BLEService> services)
        {
            _knownServices.AddRange(services);
        }

        public void HandleServicesDiscovered(BluetoothGatt device)
        {
            var existingDevice = _discoveredDevices.Where(dev => dev.DeviceAddress == device.Device.Address).FirstOrDefault();
            existingDevice.Services.Clear();
            existingDevice.AllCharacteristics.Clear();

            device.RequestMtu(255);

            var services = device.Services;
            foreach (var service in services)
            {
                var knownService = _knownServices.Where(srvc => srvc.Id.ToLower() == service.Uuid.ToIdString()).FirstOrDefault();
                if (knownService != null)
                {
                    var gatService = new BLEService(knownService.Id, knownService.Name);
                    Debug.WriteLine($"KNOWN SERVICe => {knownService.Id} - {knownService.Name} - {service.Characteristics.Count} count");

                    foreach (var characteristic in service.Characteristics)
                    {
                        Debug.WriteLine($"CHAR => {characteristic.Uuid.ToString()}");

                        var knownCharacteristic = knownService.Characteristics.Where(chr => chr.Id.ToLower() == characteristic.Uuid.ToIdString()).FirstOrDefault();
                        if (knownCharacteristic != null)
                        {
                            var properties = new List<BLECharacteristicPropertyTypes>();
                            if ((characteristic.Properties & GattProperty.Broadcast) == GattProperty.Broadcast) properties.Add(BLECharacteristicPropertyTypes.Broadcast);
                            if ((characteristic.Properties & GattProperty.ExtendedProps) == GattProperty.ExtendedProps) properties.Add(BLECharacteristicPropertyTypes.ExtendedProperties);
                            if ((characteristic.Properties & GattProperty.Indicate) == GattProperty.Indicate) properties.Add(BLECharacteristicPropertyTypes.Indicate);
                            if ((characteristic.Properties & GattProperty.Notify) == GattProperty.Notify) properties.Add(BLECharacteristicPropertyTypes.Notify);
                            if ((characteristic.Properties & GattProperty.Read) == GattProperty.Read) properties.Add(BLECharacteristicPropertyTypes.Read);
                            if ((characteristic.Properties & GattProperty.SignedWrite) == GattProperty.SignedWrite) properties.Add(BLECharacteristicPropertyTypes.SignedWrite);
                            if ((characteristic.Properties & GattProperty.Write) == GattProperty.Write) properties.Add(BLECharacteristicPropertyTypes.Write);
                            if ((characteristic.Properties & GattProperty.WriteNoResponse) == GattProperty.WriteNoResponse) properties.Add(BLECharacteristicPropertyTypes.WriteNoResponse);

                            var gatCharacteristic = new BLECharacteristic(knownService, knownCharacteristic.Id, knownCharacteristic.Name, knownCharacteristic.Type, properties);
                            gatService.Characteristics.Add(gatCharacteristic);
                            existingDevice.AllCharacteristics.Add(gatCharacteristic);

                            if (characteristic.Uuid.ToString() == STATE_CHARACTERISTICS_UUID.ToString())
                            {
                                device.SetCharacteristicNotification(characteristic, true);
                            }
                        }
                    }
                    existingDevice.Services.Add(gatService);
                }
            }
        }


        public void HandleDeviceConnected(BluetoothGatt device)
        {
            if (!_bleDevices.Where(dvc => dvc.Device.Address == device.Device.Address).Any())
                _bleDevices.Add(device);

            _dispatcherService.Invoke(() =>
            {
                var existingDevice = _discoveredDevices.Where(dev => dev.DeviceAddress == device.Device.Address).FirstOrDefault();
                existingDevice.Connected = true;
                lock (ConnectedDevices)
                {
                    if (!ConnectedDevices.Where(cd => cd.DeviceAddress == existingDevice.DeviceAddress).Any())
                    {
                        ConnectedDevices.Add(existingDevice);
                    }
                }

                DeviceConnected?.Invoke(this, existingDevice);
            });
        }

        public void HandleDeviceDisconnected(BluetoothGatt device)
        {
            BluetoothGattService service = device.GetService(NUVIOT_SRVC_UUID);
            BluetoothGattCharacteristic characteristics = service.GetCharacteristic(STATE_CHARACTERISTICS_UUID);
            device.SetCharacteristicNotification(characteristics, false);

            _dispatcherService.Invoke(() =>
            {
                var existingDevice = _discoveredDevices.Where(dev => dev.DeviceAddress == device.Device.Address).FirstOrDefault();
                existingDevice.Connected = false;
                _bleDevices.Remove(device);

                lock (ConnectedDevices)
                {
                    if (ConnectedDevices.Contains(existingDevice))
                    {
                        ConnectedDevices.Remove(existingDevice);
                    }
                }

                DeviceDisconnected?.Invoke(this, existingDevice);
            });
        }

        public void HandleDeviceDiscovered(BluetoothDevice androidBLEdevice)
        {
            var device = new BLEDevice()
            {
                DeviceAddress = androidBLEdevice.Address,
                DeviceName = androidBLEdevice.Name,
            };

            lock (_androidDevices)
            {
                if (!_androidDevices.Where(and => and.Address == androidBLEdevice.Address).Any())
                {
                    _androidDevices.Add(androidBLEdevice);
                }
            }

            _dispatcherService.Invoke(() =>
            {
                lock (_discoveredDevices)
                {
                    var existingDevice = _discoveredDevices.Where(dev => dev.DeviceAddress == device.DeviceAddress).FirstOrDefault();
                    if (existingDevice != null)
                    {
                        existingDevice.LastSeen = DateTime.Now;
                    }
                    else
                    {
                        device.LastSeen = DateTime.Now;
                        _discoveredDevices.Add(device);
                    }

                    DeviceDiscovered?.Invoke(this, device);
                }
            });
        }

        public void CharacteristicsRead(String uuid)
        {
            _charactersiticRead.Release();
        }

        public void CharacteristicsWrote(String uuid)
        {
            _charactersiticWrote.Release();
        }

        public Task ConnectAsync(BLEDevice device)
        {
            var androidDevice = _androidDevices.Where(dev => dev.Address == device.DeviceAddress).FirstOrDefault();
            if (androidDevice != null)
            {
                androidDevice.ConnectGatt(Android.App.Application.Context, true, _gattCallback);
            }

            return Task.CompletedTask;
        }

        public Task DisconnectAsync(BLEDevice device)
        {
            var androidDevice = _bleDevices.Where(dev => dev.Device.Address == device.DeviceAddress).FirstOrDefault();
            if (androidDevice != null)
            {
                androidDevice.Disconnect();
            }

            return Task.CompletedTask;
        }

        public Task StartScanAsync()
        {
            if (!IsScanning)
            {
                _bluetoothLeScanner.StartScan(_scanCallbackHanndler);
                IsScanning = true;
            }

            return Task.CompletedTask;
        }

        public Task StopScanAsync()
        {
            if (IsScanning)
            {
                _bluetoothLeScanner.StopScan(_scanCallbackHanndler);
                IsScanning = false;
            }
            return Task.CompletedTask;
        }

        public ObservableCollection<BLEDevice> DiscoveredDevices
        {
            get => _discoveredDevices;
        }

        public ObservableCollection<BLEDevice> ConnectedDevices
        {
            get => _connectedDevices;
        }

        private BluetoothGattCharacteristic Find(BluetoothGatt device, BLEService service, BLECharacteristic characteristic)
        {
            var deviceService = device.Services.Where(srvc => srvc.Uuid.ToIdString() == service.Id).FirstOrDefault();
            if (deviceService == null)
            {
                return null;
            }

            return deviceService.Characteristics.Where(chr => chr.Uuid.ToIdString() == characteristic.Id).FirstOrDefault();
        }

        private const String CCC_DESCRIPTOR_UUID = "00002902-0000-1000-8000-00805f9b34fb";

        public Task<bool> SubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            var androidDevice = _bleDevices.Where(dev => dev.Device.Address == device.DeviceAddress).FirstOrDefault();

            var gattCharacteristic = Find(androidDevice, service, characteristic);

            if (gattCharacteristic != null)
            {
                var enabled = androidDevice.SetCharacteristicNotification(gattCharacteristic, true);
                if (enabled)
                {
                    BluetoothGattDescriptor descriptor = gattCharacteristic.GetDescriptor(UUID.FromString(CCC_DESCRIPTOR_UUID));
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                    return Task.FromResult(androidDevice.WriteDescriptor(descriptor));
                }
            }

            return Task.FromResult(false);

        }

        public Task<bool> UnsubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            var androidDevice = _bleDevices.Where(dev => dev.Device.Address == device.DeviceAddress).FirstOrDefault();

            var gattCharacteristic = Find(androidDevice, service, characteristic);
            if (gattCharacteristic != null)
            {
                var disabled = androidDevice.SetCharacteristicNotification(gattCharacteristic, false);
                if (disabled)
                {
                    BluetoothGattDescriptor descriptor = gattCharacteristic.GetDescriptor(UUID.FromString(CCC_DESCRIPTOR_UUID));
                    descriptor.SetValue(BluetoothGattDescriptor.DisableNotificationValue.ToArray());
                    return Task.FromResult(androidDevice.WriteDescriptor(descriptor));
                }
            }

            return Task.FromResult(false);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, string str)
        {
            var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(str);
            return WriteCharacteristic(device, service, characteristic, buffer);
        }

        private void PopulateValue(BLECharacteristic characteristic, byte[] buffer)
        {
            switch (characteristic.Type)
            {
                case BLECharacteristicType.Enum:
                    break;
                case BLECharacteristicType.Boolean:
                case BLECharacteristicType.String:
                    characteristic.Value = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
                    break;
                case BLECharacteristicType.StringArray:
                    break;
                case BLECharacteristicType.Integer:
                    break;
                case BLECharacteristicType.Real:
                    break;
                case BLECharacteristicType.IntegerArray:
                    break;
                case BLECharacteristicType.RealArray:
                    break;
                case BLECharacteristicType.ByteArray:
                    break;
                case BLECharacteristicType.Command:
                    break;
            }
        }

        public async Task<byte[]> ReadCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            var androidDevice = _bleDevices.Where(dev => dev.Device.Address == device.DeviceAddress).FirstOrDefault();

            var gatCharacteristic = Find(androidDevice, service, characteristic);
            if (gatCharacteristic != null)
            {
                if (androidDevice.ReadCharacteristic(gatCharacteristic))
                {
                    if (await _charactersiticRead.WaitAsync(2500))
                    {
                        var result = gatCharacteristic.GetValue();
                        characteristic.Buffer = result;
                        PopulateValue(characteristic, result);

                        Debug.WriteLine($"Received value: {characteristic.Name} - {characteristic.Value}");

                        return result;
                    }
                }
            }

            return null;
        }

        public async Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, byte[] value)
        {
            var androidDevice = _bleDevices.Where(dev => dev.Device.Address == device.DeviceAddress).FirstOrDefault();

            var gatCharacteristic = Find(androidDevice, service, characteristic);
            if (gatCharacteristic != null)
            {
                gatCharacteristic.SetValue(value);
                if (androidDevice.WriteCharacteristic(gatCharacteristic))
                {
                    if (await _charactersiticWrote.WaitAsync(2500))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Task<bool> UpdateCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return WriteCharacteristic(device, service, characteristic, characteristic.Buffer);
        }
    }

    public static class GattExtensions
    {
        public static string ToIdString(this UUID? value)
        {
            return value.ToString().ToLower().Trim('{', '}');
        }
    }

}
