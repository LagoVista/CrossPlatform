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
            _manager.DiscoveredPeripheral += _manager_DiscoveredPeripheral;
            _manager.ConnectedPeripheral += _manager_ConnectedPeripheral;
            _manager.DisconnectedPeripheral += _manager_DisconnectedPeripheral;
        }

        public ObservableCollection<BLEDevice> DiscoveredDevices { get; } = new ObservableCollection<BLEDevice>();

        public ObservableCollection<BLEDevice> ConnectedDevices { get; } = new ObservableCollection<BLEDevice>();

        private List<CBPeripheral> _periperals { get; } = new List<CBPeripheral>();
        private List<CBPeripheral> _connectedPeriperals { get; } = new List<CBPeripheral>();

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
            await _deviceAccessLocker.WaitAsync();
            var periperal = _periperals.FirstOrDefault(per => per.Identifier.ToString() == device.DeviceAddress);
            if (periperal != null)
            {
                _manager.ConnectPeripheral(periperal);
            }

            _deviceAccessLocker.Release();
        }

        public async Task DisconnectAsync(BLEDevice device)
        {
            await _deviceAccessLocker.WaitAsync();
            var periperal = _periperals.FirstOrDefault(per => per.Identifier.ToString() == device.DeviceAddress);
            
            if (periperal != null)
            {
                _manager.CancelPeripheralConnection(periperal);
            }

            _deviceAccessLocker.Release();
        }

        public Task<byte[]> ReadCharacteristicAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task<byte[]>.FromResult(new byte[20]);
        }

        public void RegisterKnownServices(IEnumerable<BLEService> services)
        {

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

        private async void _manager_ConnectedPeripheral(object sender, CBPeripheralEventArgs e)
        {
            await _deviceAccessLocker.WaitAsync();
            Debug.WriteLine("Periperal Connected.");

            e.Peripheral.DiscoveredService += Peripheral_DiscoveredService;
            e.Peripheral.DiscoveredCharacteristic += Peripheral_DiscoveredCharacteristic;

            _connectedPeriperals.Add(e.Peripheral);

            if (e.Peripheral.Services == null)
            {                
                e.Peripheral.DiscoverServices();                
            }

            var bleDevice = DiscoveredDevices.FirstOrDefault(devc => devc.DeviceAddress == e.Peripheral.Identifier.ToString());
            if(bleDevice != null)
            {
                DeviceConnected?.Invoke(this, bleDevice);
                ConnectedDevices.Add(bleDevice);
            }

            _deviceAccessLocker.Release();
        }

        private void Peripheral_DiscoveredCharacteristic(object sender, CBServiceEventArgs e)
        {
            foreach (var characteristics in e.Service.Characteristics)
            {
                Debug.WriteLine($"\t\t{characteristics.UUID}");
            }
        }

        private void Peripheral_DiscoveredService(object sender, NSErrorEventArgs e)
        {
            var peripheral = sender as CBPeripheral;
            foreach(var srvc in peripheral.Services)
            {
                peripheral.DiscoverCharacteristics(srvc);
            }
        }

        private async void _manager_DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e)
        {
            await _deviceAccessLocker.WaitAsync();
            try
            {
                var device = new BLEDevice()
                {
                    DeviceAddress = e.Peripheral.Identifier.ToString(),
                    DeviceName = e.AdvertisementData.ValueForKey(new NSString("kCBAdvDataLocalName")).ToString(),
                };

                Debug.WriteLine($"{device.DeviceAddress} - {device.DeviceName}");

                if(!_periperals.Any(per=>per.Identifier == e.Peripheral.Identifier))
                {
                    _periperals.Add(e.Peripheral);
                }

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
            finally
            {
                _deviceAccessLocker.Release();
            }
        }

        private async void _manager_DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e)
        {
            await _deviceAccessLocker.WaitAsync();
            Debug.WriteLine("Periperal Connected.");

            var peripheral = sender as CBPeripheral;
            peripheral.DiscoveredService -= Peripheral_DiscoveredService;
            peripheral.DiscoveredCharacteristic -= Peripheral_DiscoveredCharacteristic;

            _connectedPeriperals.Remove(e.Peripheral);

            var bleDevice = DiscoveredDevices.FirstOrDefault(devc => devc.DeviceAddress == peripheral.Identifier.ToString());
            if (bleDevice != null)
            {
                DeviceDisconnected(this, bleDevice);
                ConnectedDevices.Remove(bleDevice);
            }

            _deviceAccessLocker.Release();
        }

        public Task StopScanAsync()
        {
            IsScanning = false;

            _manager.StopScan();
            return Task.CompletedTask;
        }

        public Task<bool> SubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task<bool>.FromResult(false);
        }

        public Task<bool> UnsubscribeAsync(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task<bool>.FromResult(false);
        }

        public Task<bool> UpdateCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic)
        {
            return Task<bool>.FromResult(false);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, string str)
        {
            return Task<bool>.FromResult(false);
        }

        public Task<bool> WriteCharacteristic(BLEDevice device, BLEService service, BLECharacteristic characteristic, byte[] str)
        {
            return Task<bool>.FromResult(false);
        }
    }
}