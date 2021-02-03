using InTheHand.Net.Sockets;
using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.AppLoader.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        BluetoothClient _client;

        public BluetoothSerial()
        {
            _client = new BluetoothClient();
        }

        public BTDevice CurrentDevice => throw new NotImplementedException();

        public bool IsConnected => false;

        public event EventHandler<string> ReceivedLine;
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

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ObservableCollection<BTDevice>> SearchAsync()
        {
            var btDevices = new ObservableCollection<BTDevice>();

            var devices =  _client.DiscoverDevices();
            foreach(var device in devices)
            {
                var btDevice = new BTDevice()
                {
                    DeviceId = device.DeviceAddress.ToString(),
                    DeviceName = device.DeviceName
                };
            }

            return Task.FromResult(btDevices);
        }

        public Task SendAsync(string msg)
        {
            throw new NotImplementedException();
        }

        public Task SendDFUAsync(byte[] firmware)
        {
            throw new NotImplementedException();
        }
    }
}
