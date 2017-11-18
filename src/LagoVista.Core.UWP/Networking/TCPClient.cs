using LagoVista.Client;
using LagoVista.Client.Core.Net;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LagoVista.Core.UWP.Networking
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

        public Task CloseAsync()
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

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
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

        public async Task<int> ReadAsync(byte[] buffer)
        {
            try
            {
                var charBuffer = new char[buffer.Length];
                var readTask = _reader.ReadAsync(charBuffer, 0, charBuffer.Length);
                readTask.Wait(_cancelListenerSource.Token);
                var bytesRead = await readTask;

                buffer = charBuffer.Select(ch => (byte)ch).ToArray();

                return bytesRead;
            }
            catch (TaskCanceledException)
            {
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public Task<byte[]> ReceiveAsync()
        {
            throw new NotImplementedException();
        }

        public async Task WriteAsync(byte[] buffer, int start, int length)
        {
            await _writer.WriteAsync(buffer.Select(ch => (char)ch).ToArray(), start, length);
            await _writer.FlushAsync(); ;
        }

        public Task WriteAsync(string msg)
        {
            var bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
            return WriteAsync(bytes, 0, bytes.Length);
        }

        public Task<int> WriteAsync<T>(T obj) where T : class
        {
            throw new NotImplementedException();
        }

        Task<int> ITCPClient.WriteAsync(byte[] buffer, int start, int length)
        {
            throw new NotImplementedException();
        }

        Task<int> ITCPClient.WriteAsync(string output)
        {
            throw new NotImplementedException();
        }
    }
}
