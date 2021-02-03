using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.WPF.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        public BTDevice CurrentDevice => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

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
            throw new NotImplementedException();
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
