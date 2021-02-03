using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.XPlat.WPF.Services
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
}
