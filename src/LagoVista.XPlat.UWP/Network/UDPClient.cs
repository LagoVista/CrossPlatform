using LagoVista.Client.Core;
using LagoVista.Client.Core.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.XPlat.UWP.Network
{
    public class UDPClient : IUDPClient
    {
        CancellationTokenSource _cancelListenerSource;

        int _port;
        System.Net.Sockets.UdpClient _udpClient;

        IPEndPoint _serverEndPoint;

        public UDPClient()
        {

        }

        public async Task ConnectAsync(String endpoint, int port)
        {
            _cancelListenerSource = new CancellationTokenSource();

            _port = port;
            IPAddress ipAddress;

            if (!IPAddress.TryParse(endpoint, out ipAddress))
            {
                var addr = await Dns.GetHostEntryAsync(endpoint);
                ipAddress = addr.AddressList.Where(adr=>adr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();
            }

            _serverEndPoint = new IPEndPoint(ipAddress, port);

            _udpClient = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Any, _port));            
        }

        public Task<byte[]> ReceiveAsync()
        {
            var recvTask = _udpClient.ReceiveAsync();
            recvTask.Wait(_cancelListenerSource.Token);

            _cancelListenerSource.Token.ThrowIfCancellationRequested();
            return Task.FromResult(recvTask.Result.Buffer);
        }

        public Task<int> WriteAsync(byte[] buffer, int start, int length)
        {
            return _udpClient.SendAsync(buffer, buffer.Length, _serverEndPoint);
        }

        public Task DisconnectAsync()
        {
            if(_udpClient != null)
            {
                _cancelListenerSource.Cancel();
                _cancelListenerSource = null;
                _udpClient.Dispose();
                _udpClient = null;
            }

            return Task.FromResult(default(object));
        }

        public Task<int> WriteAsync(string output)
        {
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(output);
            return WriteAsync(bytes, 0, bytes.Length);
        }

        public Task<int> WriteAsync<T>(T obj) where T : class
        {
            var json = JsonConvert.SerializeObject(obj);
            return WriteAsync(json);
        }



        public void Dispose()
        {
            if(_udpClient != null)
            {
                DisconnectAsync();
            }
        }
    }
}
