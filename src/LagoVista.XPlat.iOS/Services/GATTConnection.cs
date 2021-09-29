using CoreBluetooth;
using Foundation;
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
using UIKit;

namespace LagoVista.XPlat.iOS.Services
{
    public class GATTConnection : IGATTConnection
    {
        private readonly SemaphoreSlim _deviceAccessLocker = new SemaphoreSlim(1, 1);
        private readonly IDispatcherServices _dispatcherService;
        private readonly CBCentralManager _manager = new CBCentralManager();

        public bool IsScanning
        {
            get; private set;
        }

        public GATTConnection(IDispatcherServices dispatcher)
        {
            _dispatcherService = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _manager.DiscoveredPeripheral += Manager_DiscoveredPeripheral;
            _manager.ConnectedPeripheral += Manager_ConnectedPeripheral;
            _manager.DisconnectedPeripheral += Manager_DisconnectedPeripheral;
        }

        public ObservableCollection<BLEDevice> DiscoveredDevices { get; } = new ObservableCollection<BLEDevice>();


        // Similar to ConnectedDevices, but this one is thread safe, ConnectedDevices is mainly for display and control in the UI and is not
        // critical to operations.
        private readonly List<BLEDevice> _internalConnectedDevices = new List<BLEDevice>();
        public ObservableCollection<BLEDevice> ConnectedDevices { get; } = new ObservableCollection<BLEDevice>();

        // Maintain a list of devices that are requested to be connected to but have not connected.
        // this makes sure that connection attempts only happen once per peripheral.
        private readonly List<BLEDevice> _connectingDevices = new List<BLEDevice>();

        private readonly Dictionary<string, CBPeripheral> _periperals = new Dictionary<string, CBPeripheral>();
        private readonly Dictionary<string, CBPeripheral> _connectedPeriperals = new Dictionary<string, CBPeripheral>();

        private readonly Dictionary<string, Dictionary<string, CBCharacteristic>> _allCharacteristics = new Dictionary<string, Dictionary<string, CBCharacteristic>>();
        private readonly Dictionary<string, Dictionary<string, CBService>> _allDeviceServices = new Dictionary<string, Dictionary<string, CBService>>();

        private readonly List<BLEService> _knownServices = new List<BLEService>();

        public event EventHandler<BLEDevice> DeviceDiscovered;
        public event EventHandler<BLEDevice> DeviceConnected;
        public event EventHandler<BLEDevice> DeviceDisconnected;
        public event EventHandler<BLECharacteristicsValue> CharacteristicChanged;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler<string> DFUFailed;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> ReceiveConsoleOut;

        private async void WatchdogTimer_Tick(object sender, EventArgs e)
        {
            await _deviceAccessLocker.WaitAsync();
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
                    PrivateDisconnect(device);
                }

                devicesToRemove.Clear();

                foreach (var connectedDevice in _internalConnectedDevices.ToList())
                {
                    var iosDevice = _periperals[connectedDevice.DeviceAddress];
                    if (iosDevice != null && iosDevice.IsConnected)
                    {
                        try
                        {
                            var pingCharacteristics = _allCharacteristics[connectedDevice.DeviceAddress][NuvIoTGATTProfile.CHAR_UUID_STATE];
                            var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes("ping");
                            iosDevice.WriteValue(NSData.FromArray(buffer), pingCharacteristics, CBCharacteristicWriteType.WithResponse);
                        }
                        catch
                        {
                            Debug.WriteLine($"!===> Tick - exception sending ping to - {connectedDevice.DeviceName}");
                            PrivateDisconnect(connectedDevice);
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

        public async Task ConnectAsync(BLEDevice device)
        {
            try
            {
                Debug.WriteLine($"=> ConnectAsync - Start connecting Devices {device}");
                await _deviceAccessLocker.WaitAsync();
                var periperal = _periperals[device.DeviceAddress];
                _manager.ConnectPeripheral(periperal);
                Debug.WriteLine($"=> ConnectAsync - Finish connecting Devices {device}");
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        public async Task DisconnectAsync(BLEDevice device)
        {
            try
            {
                Debug.WriteLine($"=> DisconnectAsync - Start disconnecting Devices {device}");
                await _deviceAccessLocker.WaitAsync();
                var periperal = _periperals[device.DeviceAddress];
                _manager.CancelPeripheralConnection(periperal);
                Debug.WriteLine($"=> DiconnectAsync - Finish disconnecting Devices {device}");
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        public async Task<byte[]> ReadCharacteristicAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();

                var periperal = _periperals[device.DeviceAddress];

                var deviceCharacteristic = _allCharacteristics[device.DeviceAddress][characteristic.Id];
                periperal.ReadValue(deviceCharacteristic);
                return deviceCharacteristic.Value.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        public void RegisterKnownServices(IEnumerable<BLEService> services)
        {
            _knownServices.AddRange(services);
        }

        public Task StartScanAsync()
        {
            IsScanning = true;
            var serviceUUIDS = new List<CBUUID>()
            {
                CBUUID.FromString("d804b639-6ce7-4e80-9f8a-ce0f699085eb")
            };

            _manager.ScanForPeripherals(serviceUUIDS.ToArray());

            return Task.CompletedTask;
        }

        private async void Manager_ConnectedPeripheral(object sender, CBPeripheralEventArgs e)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();
                Debug.WriteLine("=> Peripheral Connected.");

                e.Peripheral.DiscoveredService += Peripheral_DiscoveredService;
                e.Peripheral.DiscoveredCharacteristic += Peripheral_DiscoveredCharacteristic;
                e.Peripheral.UpdatedCharacterteristicValue += Periperal_UpdatedCharacterteristicValue;

                _connectedPeriperals.Add(e.Peripheral.Identifier.ToString(), e.Peripheral);
                _allDeviceServices.Add(e.Peripheral.Identifier.ToString(), new Dictionary<string, CBService>());
                _allCharacteristics.Add(e.Peripheral.Identifier.ToString(), new Dictionary<string, CBCharacteristic>());

                if (e.Peripheral.Services == null)
                {
                    Debug.WriteLine("==> Peripheral Connected - Discovering services.");
                    e.Peripheral.DiscoverServices();
                }
                else
                {
                    Debug.WriteLine("==> Peripheral Connected - Services already discovered.");
                }

                var bleDevice = DiscoveredDevices.FirstOrDefault(devc => devc.DeviceAddress == e.Peripheral.Identifier.ToString());
                _internalConnectedDevices.Add(bleDevice);
                if (bleDevice != null)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        DeviceConnected?.Invoke(this, bleDevice);
                        ConnectedDevices.Add(bleDevice);
                    });
                }

                Debug.WriteLine("=> Peripheral Connected - Success.");
            }
            finally
            {
                _deviceAccessLocker.Release();
                Debug.WriteLine("=> Peripheral Connected - Finish.");
            }
        }

        private async void Peripheral_DiscoveredCharacteristic(object sender, CBServiceEventArgs e)
        {
            await _deviceAccessLocker.WaitAsync();
            try
            {
                foreach (var characteristic in e.Service.Characteristics)
                {
                    _allCharacteristics[e.Service.Peripheral.Identifier.ToString()].Add(characteristic.UUID.ToString(), characteristic);
                    Debug.WriteLine($"\t\t{characteristic.UUID}");
                }
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        private async void Peripheral_DiscoveredService(object sender, NSErrorEventArgs e)
        {
            await _deviceAccessLocker.WaitAsync();
            try
            {
                var peripheral = sender as CBPeripheral;
                foreach (var srvc in peripheral.Services)
                {
                    peripheral.DiscoverCharacteristics(srvc);
                    _allDeviceServices[peripheral.Identifier.ToString()].Add(srvc.UUID.ToString(), srvc);
                    Debug.WriteLine($"\t\t{srvc.UUID}");
                }
            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        private async void Manager_DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();

                var device = new BLEDevice()
                {
                    DeviceAddress = e.Peripheral.Identifier.ToString(),
                    DeviceName = e.AdvertisementData.ValueForKey(new NSString("kCBAdvDataLocalName")).ToString(),
                };
          
                if (!_periperals.ContainsKey(e.Peripheral.Identifier.ToString()))
                {
                    _periperals.Add(e.Peripheral.Identifier.ToString(), e.Peripheral);
                    Debug.WriteLine($"===> Peripheral New Device Discovered - {device.DeviceName} - {device.DeviceAddress}");
                }
                else
                {
                    Debug.WriteLine($"===> Peripheral Existing Device Rediscovered - {device.DeviceName} - {device.DeviceAddress}");
                }

                _dispatcherService.Invoke(() =>
                {
                    device.LastSeen = DateTime.Now;
                    var existingDevice = DiscoveredDevices.Where(dev => dev.DeviceAddress == device.DeviceAddress).FirstOrDefault();
                    if (existingDevice == null)
                    {
                        DiscoveredDevices.Add(device);
                    }

                    DeviceDiscovered?.Invoke(this, device);
                });

            }
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        /// <summary>
        /// This method is to be called from other methods that have
        /// already obtained a lock
        /// </summary>
        private void PrivateDisconnect(BLEDevice device)
        {

            if (_deviceAccessLocker.CurrentCount > 0)
            {
                throw new InvalidOperationException("Attempt to disconnect while not in access locker.");
            }

            try
            {
                Debug.WriteLine("===> Peripheral Disconnecting - Start");
                var peripheral = _connectedPeriperals[device.DeviceAddress];
                peripheral.DiscoveredService -= Peripheral_DiscoveredService;
                peripheral.DiscoveredCharacteristic -= Peripheral_DiscoveredCharacteristic;
                peripheral.UpdatedCharacterteristicValue -= Periperal_UpdatedCharacterteristicValue;
                foreach (var dvcsrvc in _allDeviceServices[device.DeviceAddress].Values)
                {
                    dvcsrvc.Dispose();
                }
                _allDeviceServices.Remove(device.DeviceAddress);

                foreach (var chr in _allCharacteristics[device.DeviceAddress].Values)
                {
                    chr.Dispose();
                }
                _allCharacteristics.Remove(device.DeviceAddress);

                _connectedPeriperals.Remove(device.DeviceAddress);
                peripheral.Dispose();

                _internalConnectedDevices.Remove(device);

                _dispatcherService.Invoke(() =>
                {
                    DeviceDisconnected?.Invoke(this, device);
                    ConnectedDevices.Remove(device);
                });
            
                Debug.WriteLine("===> Peripheral Disconnecting - Finish");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"===> Peripheral Disconnecting - Fail - {ex.Message}");
            }
        }

        private async void Manager_DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();
                Debug.WriteLine("=> Peripheral Disconnected - Start.");
                var peripheral = sender as CBPeripheral;
                var bleDevice = ConnectedDevices.FirstOrDefault(cd=>cd.DeviceAddress == peripheral.Identifier.ToString());
                PrivateDisconnect(bleDevice);
                Debug.WriteLine("=> Peripheral Disconnected - Success.");
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"=> Peripheral Disconnected - Failed - {ex.Message}");
            }
            finally
            {
                Debug.WriteLine("=> Peripheral Disconnected - Finish.");
                _deviceAccessLocker.Release();
            }
        }

        public Task StopScanAsync()
        {
            IsScanning = false;

            _manager.StopScan();
            return Task.CompletedTask;
        }

        public async Task<bool> SubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();
                Debug.WriteLine("=> Subscribe - Start");
                var deviceCharacteristic = _allCharacteristics[device.DeviceAddress][characteristic.ToString()];
               
                var peripheral = _periperals[device.DeviceAddress];
                peripheral.SetNotifyValue(true, deviceCharacteristic);
                Debug.WriteLine("=> Subscribe - Success");
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"=> Subscribe - Failed - {ex.Message}");
                PrivateDisconnect(device);
                return false;
            }
            finally
            {
                Debug.WriteLine("=> Subscribe - Finish.");
                _deviceAccessLocker.Release();
            }
        }

        private void Periperal_UpdatedCharacterteristicValue(object sender, CBCharacteristicEventArgs e)
        {
            _dispatcherService.Invoke(() =>
            {
                this.CharacteristicChanged(this, new BLECharacteristicsValue()
                {
                    Uid = e.Characteristic.UUID.ToString(),
                    Value = System.Text.ASCIIEncoding.ASCII.GetString(e.Characteristic.Value.ToArray())
                }); 
            });
        }

        public async Task<bool> UnsubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();
                Debug.WriteLine("=> Unsubscribe - Start.");
                var deviceCharacteristic = _allCharacteristics[device.DeviceAddress][characteristic.ToString()];
                var periperal = _periperals[device.DeviceAddress];
                periperal.SetNotifyValue(false, deviceCharacteristic);
                Debug.WriteLine("=> Unsubscribe - Success");
                return true;
            }
            catch(Exception ex)
            {
                PrivateDisconnect(device);
                Debug.WriteLine($"=> Unsubscribe - Failed - {ex.Message}");
                return false;
            }
            finally
            {
                Debug.WriteLine("=> Unsubscribe - Finish.");
                _deviceAccessLocker.Release();
            }
        }

        public Task<bool> UpdateCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task<bool>.FromResult(false);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, string str)
        {
            return WriteCharacteristic(device, service, characteristic, System.Text.ASCIIEncoding.ASCII.GetBytes(str));
        }

        public async Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, byte[] str)
        {
            try
            {
                await _deviceAccessLocker.WaitAsync();
                Debug.WriteLine("=> Write Characteristic - Start.");
                var deviceCharacteristic = _allCharacteristics[device.DeviceAddress][characteristic.ToString()];
                var periperal = _periperals[device.DeviceAddress];
                periperal.WriteValue(NSData.FromArray(str), deviceCharacteristic, CBCharacteristicWriteType.WithResponse);
                Debug.WriteLine("=> Write Characteristic - Success.");
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"=> Write Characteristic - Failed - {ex.Message}");
                PrivateDisconnect(device);

                return false;
            }
            finally
            {
                _deviceAccessLocker.Release();
                Debug.WriteLine("=> Write Characteristic - Finish.");
            }
        }
    }
}