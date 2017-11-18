using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using LagoVista.Core.Validation;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Diagnostics;
using System.Threading;

namespace LagoVista.XPlat.iOS.Services
{
    public class WebSocket : LagoVista.Client.Core.Net.IWebSocket
    {
        public event EventHandler<string> MessageReceived;
        public event EventHandler Closed;

        ClientWebSocket _webSocket;

        private const int BUFFER_SIZE = 4096;
        private const int BUFFER_SCALER = 20;

        System.Threading.CancellationTokenSource _cancelTokenSource;
        bool _running = false;

        public Task<InvokeResult> CloseAsync()
        {
            _running = false;
            _cancelTokenSource.Cancel();
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
                _webSocket = new ClientWebSocket();

                _cancelTokenSource = new System.Threading.CancellationTokenSource();

                await _webSocket.ConnectAsync(uri, System.Threading.CancellationToken.None);
                Debug.WriteLine("WE ARE OPEN!");
                _running = true;
                StartReceiveThread();

                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                return InvokeResult.FromException("WebSocket_OpenAsync", ex);
            }
        }

        public void StartReceiveThread()
        {
            Task.Run(async () =>
            {
                var temporaryBuffer = new byte[BUFFER_SIZE];
                var buffer = new byte[BUFFER_SIZE * BUFFER_SCALER];
                var offset = 0;

                while (_running && _webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult response;

                    do
                    {
                        response = await _webSocket.ReceiveAsync(new ArraySegment<byte>(temporaryBuffer), _cancelTokenSource.Token);
                        temporaryBuffer.CopyTo(buffer, offset);
                        offset += response.Count;
                        temporaryBuffer = new byte[BUFFER_SIZE];
                    } while (!response.EndOfMessage);

                    if (response.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close response received", CancellationToken.None);
                    }
                    else
                    {
                        var result = Encoding.UTF8.GetString(buffer);
                        MessageReceived?.Invoke(this, result);
                        buffer = new byte[BUFFER_SIZE * BUFFER_SCALER];
                        offset = 0;
                    }
                }

                Closed?.Invoke(this, null);

            }, _cancelTokenSource.Token);
        }

    }
}