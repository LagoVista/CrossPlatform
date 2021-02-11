using LagoVista.Client.Core.Net;
using LagoVista.Core.Validation;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AppLoader.Services
{
    public class WebSocket : IWebSocket
    {
        ClientWebSocket _clientWebSocket;
        bool _notCancelled;

        CancellationTokenSource _tokenSource;

        public event EventHandler<string> MessageReceived;
        public event EventHandler Closed;

        public Task<InvokeResult> CloseAsync()
        {
            try
            {
                if (_clientWebSocket != null)
                {
                    _notCancelled = false;
                    _tokenSource.Cancel();
                }

                return Task.FromResult(InvokeResult.Success);
            }
            catch (Exception ex)
            {
                return Task.FromResult(InvokeResult.FromException("WebSocket_CloseAsync", ex));
            }
        }

        public void Dispose()
        {
            if (_clientWebSocket != null)
            {
                _clientWebSocket.Dispose();
                _clientWebSocket = null;
            }
        }

        private async void ListenAsync()
        {
            var buffer = new byte[4096];
            while (_notCancelled)
            {
                try
                {
                    var rcvBuffer = new ArraySegment<byte>(buffer);
                    var rcvResult = await _clientWebSocket.ReceiveAsync(rcvBuffer, _tokenSource.Token);
                    var msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                    var rcvMsg = Encoding.UTF8.GetString(msgBytes);
                    MessageReceived(this, rcvMsg);
                }
                catch (TaskCanceledException)
                {

                }
            }
        }

        public async Task<InvokeResult> OpenAsync(Uri uri)
        {
            _clientWebSocket = new ClientWebSocket();
            _tokenSource = new CancellationTokenSource();
            _notCancelled = true;

            await _clientWebSocket.ConnectAsync(uri, _tokenSource.Token);
            ListenAsync();
            return InvokeResult.Success;
        }
    }
}
