using System;
using System.Threading.Tasks;
using LagoVista.Client.Core.Net;
using LagoVista.Client.Core;

namespace LagoVista.XPlat.iOS.Services
{
    public class UDPClient : IUDPClient
    {
        public UDPClient()
        {
        }

        public Task ConnectAsync(string host, int port)
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> WriteAsync(byte[] buffer, int start, int length)
        {
            throw new NotImplementedException();
        }

        public Task<int> WriteAsync(string output)
        {
            throw new NotImplementedException();
        }

        public Task<int> WriteAsync<T>(T obj) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
