using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace LagoVista.Core.UWP.Services
{
    public class SerialPort : ISerialPort
    {
        SerialDevice _serialDevice;

        SerialPortInfo _portInfo;

        DataReader _dataReader;
        DataWriter _dataWriter;

        public SerialPort(SerialPortInfo info)
        {
            _portInfo = info;
        }

        public bool IsConnected
        {
            get { return _serialDevice != null; }
        }

        public Task CloseAsync()
        {
            Dispose();
            return Task.FromResult(default(object));
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_dataReader != null)
                {
                    _dataReader.Dispose();
                    _dataReader = null;
                }

                if (_dataWriter != null)
                {
                    _dataWriter.Dispose();
                    _dataWriter = null;
                }

                if (_serialDevice != null)
                {
                    _serialDevice.Dispose();
                    _serialDevice = null;
                }
            }
        }

        public async Task OpenAsync()
        {
            _serialDevice = await SerialDevice.FromIdAsync(_portInfo.Id);
            _serialDevice.BaudRate = (uint)_portInfo.BaudRate;
            _serialDevice.DataBits = 8;
            _serialDevice.Parity = SerialParity.None;
            _serialDevice.StopBits = SerialStopBitCount.One;
            _serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(100);
            _serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(100);

            _dataReader = new DataReader(_serialDevice.InputStream);
            _dataWriter = new DataWriter(_serialDevice.OutputStream);

            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
        }

        public async Task<int> ReadAsync(byte[] buffer, int start, int size, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_dataReader == null)
            {
                throw new Exception("Port not open.");
            }

            Task<UInt32> loadAsyncTask;
            cancellationToken.ThrowIfCancellationRequested();

            loadAsyncTask = _dataReader.LoadAsync((uint)size).AsTask(cancellationToken);
            UInt32 bytesRead = await loadAsyncTask;
            var maxToRead = Math.Min(size, bytesRead);
            for (var idx = 0; idx < maxToRead; ++idx)
            {
                buffer[idx] = _dataReader.ReadByte();
            }

            return (int)maxToRead;
        }

        public async Task WriteAsync(string msg)
        {
            if (_dataReader == null)
            {
                throw new Exception("Port not open.");
            }

            _dataWriter.WriteString(msg);
            var operation = await _dataWriter.StoreAsync();
            if (operation != msg.Length)
            {
                throw new Exception($"Data Storage Operation Not Successful: {operation}");
            }
        }

        public async Task WriteAsync(byte[] buffer)
        {
            if (_dataReader == null)
            {
                throw new Exception("Port not open.");
            }

            _dataWriter.WriteBytes(buffer);
            var operation = await _dataWriter.StoreAsync();
            if (operation != buffer.Length)
            {
                throw new Exception($"Data Storage Operation Not Successful: {operation}");
            }
        }
    }
}
