﻿using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Droid.Services
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