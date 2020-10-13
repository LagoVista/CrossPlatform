using System;
using LagoVista.Client.Core.Interfaces;
using CoreBluetooth;
using LagoVista.Client.Core.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LagoVista.XPlat.iOS.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        CBCentralManager _mgr;

        public BTDevice CurrentDevice => throw new NotImplementedException();

        public BluetoothSerial()
        {
            _mgr = new CBCentralManager();
            _mgr.DisconnectedPeripheral += _mgr_DisconnectedPeripheral;
            _mgr.ConnectedPeripheral += _mgr_ConnectedPeripheral;
            _mgr.DiscoveredPeripheral += _mgr_DiscoveredPeripheral; ;
        }

        private void _mgr_DiscoveredPeripheral(object sender, CBDiscoveredPeripheralEventArgs e)
        {
            Debug.WriteLine(e.Peripheral.Name);
        }

        private void _mgr_ConnectedPeripheral(object sender, CBPeripheralEventArgs e)
        {

        }

        private void _mgr_DisconnectedPeripheral1(object sender, CBPeripheralErrorEventArgs e)
        {
        }

        private void _mgr_DisconnectedPeripheral(object sender, CBPeripheralErrorEventArgs e)
        {
        }

        public event EventHandler<string> ReceivedLine;
        public event EventHandler<BTDevice> DeviceFound;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> DFUFailed;
        public event EventHandler<BTDevice> DeviceConnected;
        public event EventHandler<BTDevice> DeviceConnecting;
        public event EventHandler<BTDevice> DeviceDisconnected;

        public Task ConnectAsync(BTDevice device)
        {
            throw new NotImplementedException();
        }

        public Task<ObservableCollection<BTDevice>> SearchAsync()
        {
            //  _mgr.ScanForPeripherals( null, new PeripheralScanningOptions() );

            var spp = CBUUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
            _mgr.ScanForPeripherals(new[] { spp });

            return Task<ObservableCollection<BTDevice>>.FromResult(new ObservableCollection<BTDevice>());
        }

        public Task SendDFUAsync(BTDevice device, byte[] firmware)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(string msg)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync(BTDevice deviceId)
        {
            throw new NotImplementedException();
        }

    }
}