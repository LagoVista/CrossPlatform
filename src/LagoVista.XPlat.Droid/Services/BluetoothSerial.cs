using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Droid.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        public BTDevice CurrentDevice => null;

        public bool IsConnected => throw new NotImplementedException();

        public event EventHandler<string> ReceivedLine;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> DFUFailed;
        public event EventHandler<BTDevice> DeviceConnected;
        public event EventHandler<BTDevice> DeviceConnecting;
        public event EventHandler<BTDevice> DeviceDisconnected;
        public event EventHandler<BTDevice> DeviceDiscovered;

        public Task ConnectAsync(BTDevice device)
        {
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ObservableCollection<BTDevice>> SearchAsync()
        {
            await Task.Delay(1);
            return new ObservableCollection<BTDevice>();
        }

        public Task SendAsync(string msg)
        {
            return Task.CompletedTask;
        }

        public Task SendDFUAsync(byte[] firmware)
        {
            throw new NotImplementedException();
        }

        public Task StopSearchingAsync()
        {
            throw new NotImplementedException();
        }
    }
}