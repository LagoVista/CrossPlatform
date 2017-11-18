using System;
using System.Threading.Tasks;
using LagoVista.Core.Validation;
using Windows.Networking.Sockets;

namespace LagoVista.XPlat.UWP.Network
{
    public class WebSocket : LagoVista.Client.Core.Net.IWebSocket
    {
        public event EventHandler<string> MessageReceived;
        public event EventHandler Closed;

        MessageWebSocket _webSocket;

        public Task<InvokeResult> CloseAsync()
        {
            if (_webSocket != null)
            {
                _webSocket.Close(1000, String.Empty);
                _webSocket.Dispose();
                _webSocket = null;
            }

            return Task.FromResult(InvokeResult.Success);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> OpenAsync(Uri uri)
        {
            try
            {
                _webSocket = new MessageWebSocket();
                _webSocket.Closed += (sndr, args) => Closed?.Invoke(this, null);
                _webSocket.MessageReceived += _webSocket_MessageReceived;
                await _webSocket.ConnectAsync(uri);

                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                return InvokeResult.FromException("WebSocket_OpenAsync", ex);
            }
        }

        private void _webSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (var reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                    var msgJSON = reader.ReadString(reader.UnconsumedBufferLength);
                    MessageReceived?.Invoke(this, msgJSON);
                }
            }
            catch(Exception)
            {
                
                _webSocket.Dispose();
                _webSocket = null;
            }
        }
    }
}