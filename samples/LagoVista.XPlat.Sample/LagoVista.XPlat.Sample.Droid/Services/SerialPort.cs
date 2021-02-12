using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.Droid.Simulator.Services
{
    public class SerialPort : ISerialPort
    {
        public bool IsConnected => throw new NotImplementedException();

        public Task CloseAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task OpenAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadAsync(byte[] bufffer, int start, int size, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string msg)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }

    public class SerialPortDeviceManager : IDeviceManager
    {        
        public ISerialPort CreateSerialPort(SerialPortInfo portInfo)
        {
            throw new NotImplementedException();
        }

        public Task<ObservableCollection<SerialPortInfo>> GetSerialPortsAsync()
        {
            throw new NotImplementedException();
        }
    }
}