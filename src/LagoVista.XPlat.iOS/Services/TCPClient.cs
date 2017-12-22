using LagoVista.Client;
using LagoVista.Client.Core;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.XPlat.iOS.Services
{
    /// <summary>
    /// Very quick and dirty TPC client, need to really make this solid, but not for V1
    /// </summary>
    public class TCPClient : ITCPClient
    {
        const int MAX_BUFFER_SIZE = 1024;

        TcpClient _tcpClient;
        CancellationTokenSource _cancelListenerSource;

        public Task DisconnectAsync()
        {
            if (_cancelListenerSource != null)
            {
                _cancelListenerSource.Cancel();
                _cancelListenerSource = null;
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
                _tcpClient = null;
            }

            return Task.FromResult(default(object));
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _cancelListenerSource = new CancellationTokenSource();

            await _tcpClient.ConnectAsync(ipAddress, port);
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var charBuffer = new byte[MAX_BUFFER_SIZE];
            var bytesRead = await _tcpClient.GetStream().ReadAsync(charBuffer, 0, charBuffer.Length);

            return charBuffer.Take(bytesRead).ToArray();
        }

        public async Task<int> WriteAsync(byte[] buffer, int start, int length)
        {
            await _tcpClient.GetStream().WriteAsync(buffer, start, length);
            return length;
        }

        public Task<int> WriteAsync(string msg)
        {
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
            return WriteAsync(bytes, 0, bytes.Length);
        }

        public Task<int> WriteAsync<T>(T obj) where T : class
        {
            var json = JsonConvert.SerializeObject(obj);
            return WriteAsync(json);
        }

        public void Dispose()
        {
            DisconnectAsync();
        }
    }
}
