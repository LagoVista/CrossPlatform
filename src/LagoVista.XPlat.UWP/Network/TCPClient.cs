using LagoVista.Client;
using LagoVista.Client.Core.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LagoVista.XPlat.UWP.Network
{
    public class TCPClient : ITCPClient
    {
        const int MAX_BUFFER_SIZE = 1024;

        StreamSocket _socket;
        StreamReader _reader;
        StreamWriter _writer;
        Stream _inputStream;
        Stream _outputStream;

        CancellationTokenSource _cancelListenerSource;

        public Task DisconnectAsync()
        {
            Dispose();

            return Task.FromResult(default(object));
        }

        public async Task ConnectAsync(string ipAddress, int port)
        {
            _cancelListenerSource = new CancellationTokenSource();

            _socket = new Windows.Networking.Sockets.StreamSocket();
            var host = new Windows.Networking.HostName(ipAddress);
            await _socket.ConnectAsync(host, port.ToString());

            _inputStream = _socket.InputStream.AsStreamForRead();
            _reader = new StreamReader(_inputStream);

            _outputStream = _socket.OutputStream.AsStreamForWrite();
            _writer = new StreamWriter(_outputStream);

        }
        public async Task<byte[]> ReceiveAsync()
        {
            var charBuffer = new char[MAX_BUFFER_SIZE];
            var readTask = _reader.ReadAsync(charBuffer, 0, charBuffer.Length);
            readTask.Wait(_cancelListenerSource.Token);
            _cancelListenerSource.Token.ThrowIfCancellationRequested();

            var bytesRead = await readTask;
            return charBuffer.Select(ch => (byte)ch).ToArray();
        }

        public async Task<int> WriteAsync(byte[] buffer, int start, int length)
        {
            await _writer.WriteAsync(buffer.Select(ch => (char)ch).ToArray(), start, length);
            await _writer.FlushAsync();
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
            _cancelListenerSource.Cancel();

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }

            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }

            if (_outputStream != null)
            {
                _outputStream.Dispose();
                _outputStream = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }
        }

    }
}
