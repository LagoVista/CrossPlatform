using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.WebUI;

namespace LagoVista.Core.UWP.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        private RfcommDeviceService _service;
        private StreamSocket _socket;
        private DataWriter _dataWriterObject;
        private DataReader _dataReaderObject;

        private CancellationTokenSource _listenCancelTokenSource = new CancellationTokenSource();

        private SemaphoreSlim _msgReceivedFlag = new SemaphoreSlim(0);

        public event EventHandler<string> ReceivedLine;
        public event EventHandler<int> DFUProgress;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> DFUFailed;
        public event EventHandler<BTDevice> DeviceConnected;
        public event EventHandler<BTDevice> DeviceConnecting;
        public event EventHandler<BTDevice> DeviceDisconnected;

        BTDevice _currentDevice = null;

        private string _lastMessage = null;

        public async Task ConnectAsync(BTDevice device)
        {
            if (_currentDevice != null)
            {
                throw new InvalidOperationException("Already Connected.");
            }

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            DeviceConnecting?.Invoke(this, device); 

             _service = await RfcommDeviceService.FromIdAsync(device.DeviceId);

            _socket = new StreamSocket();

            await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
            _dataWriterObject = new DataWriter(_socket.OutputStream);
            Listen();

            _currentDevice = device;

            DeviceConnected?.Invoke(this, device);
        }

        public Task DisconnectAsync(BTDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (_currentDevice == null)
            {
                throw new InvalidOperationException("No connected.");
            }

            if (_currentDevice != device)
            {
                throw new InvalidOperationException("Attempt to close a not conected device.");
            }

            CancelReadTask();

            return Task.CompletedTask;
        }

        public async Task<ObservableCollection<BTDevice>> SearchAsync()
        {
            var deviceInfo = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            var pairedDevices = new ObservableCollection<BTDevice>();
            foreach (var device in deviceInfo)
            {
                var service = await RfcommDeviceService.FromIdAsync(device.Id);

                pairedDevices.Add(new BTDevice()
                {
                    DeviceName = service.Device.Name,
                    DeviceId = device.Id
                });
            }

            return pairedDevices;
        }

        private async Task<String> WaitForResponseAsync()
        {
            if(_currentDevice == null)
            {
                throw new InvalidOperationException("No connected.");
            }

            await _msgReceivedFlag.WaitAsync();
            return _lastMessage;
        }

        public async Task SendDFUAsync(BTDevice device, byte[] firmware)
        {
            if (_currentDevice == null)
            {
                throw new InvalidOperationException("No connected.");
            }

            try
            {
                await SendAsync("FIRMWARE\n");

                var response = await WaitForResponseAsync();
                while (response.Trim() != "ok-start")
                {
                    response = await WaitForResponseAsync();
                    Console.WriteLine("INCORRECT RESPONSE, EXPECTING ok-start : " + response);
                }

                Console.WriteLine("RECEIVED : " + response);

                var length = firmware.Length;

                _dataWriterObject.WriteByte((byte)(length >> 24));
                _dataWriterObject.WriteByte((byte)(length >> 16));
                _dataWriterObject.WriteByte((byte)(length >> 8));
                _dataWriterObject.WriteByte((byte)(length & 0xFF));

                var storeResult = await _dataWriterObject.StoreAsync();
                if(storeResult != 4)
                {
                    throw new Exception($"Should have written 4 bytes for buffer size, wrote {storeResult}");
                }

                response = await WaitForResponseAsync();

                int blockSize = 500;

                short blocks = (short)((firmware.Length / blockSize) + 1);

                _dataWriterObject.WriteByte((byte)(blocks >> 8));
                _dataWriterObject.WriteByte((byte)(blocks & 0xFF));
                storeResult = await _dataWriterObject.StoreAsync();

                response = await WaitForResponseAsync();
                if (storeResult != 2)
                {
                    throw new Exception($"Should have written 2 bytes for block size, wrote {storeResult}");
                }

                for (var idx = 0; idx < blocks; ++idx)
                {
                    var start = idx * blockSize;
                    var len = firmware.Length - start;

                    len = Math.Min(blockSize, len);

                    _dataWriterObject.WriteByte((byte)(len >> 8));
                    _dataWriterObject.WriteByte((byte)(len & 0xFF));
                    storeResult = await _dataWriterObject.StoreAsync();

                    var sendBuffer = new byte[len];
                    Array.Copy(firmware, start, sendBuffer, 0, len);

                    // Send check sum
                    byte checkSum = 0;
                    for (int ch = 0; ch < len; ch++)
                    {
                        checkSum += sendBuffer[ch];
                    }

                    _dataWriterObject.WriteBytes(sendBuffer);
                    storeResult = await _dataWriterObject.StoreAsync();

                    _dataWriterObject.WriteByte((byte)(checkSum));
                    storeResult = await _dataWriterObject.StoreAsync();

                    var blockResponse = await WaitForResponseAsync();

                    DFUProgress?.Invoke(this, (idx * 100) / blocks);
                }

                DFUCompleted?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }

                DFUFailed?.Invoke(this, ex.Message);
                DeviceDisconnected?.Invoke(this, _currentDevice);
                _currentDevice = null;
            }
        }

        public async Task SendAsync(string msg)
        {
            try
            {
                if (_currentDevice == null)
                {
                    throw new ArgumentNullException(nameof(CurrentDevice));
                }

                _dataWriterObject.WriteString(msg);

                // Launch an async task to complete the write operation
                await _dataWriterObject.StoreAsync();
            }
            catch (Exception)
            {
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }

                DeviceDisconnected?.Invoke(this, _currentDevice);
                _currentDevice = null;
                throw;
            }

        }


        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Listen()
        {
            try
            {
                _listenCancelTokenSource = new CancellationTokenSource();
                if (_socket.InputStream != null)
                {
                    _dataReaderObject = new DataReader(_socket.InputStream);
                    // keep reading the serial input
                    while (true && _socket != null)
                    {
                        await ReadAsync(_listenCancelTokenSource.Token);
                    }
                }
            }
            catch (Exception)
            {
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }

                DeviceDisconnected?.Invoke(this, _currentDevice);
                _currentDevice = null;
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            cancellationToken.ThrowIfCancellationRequested();

            _dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            loadAsyncTask = _dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                try
                {
                    _lastMessage = _dataReaderObject.ReadString(bytesRead);
                    this.ReceivedLine?.Invoke(this, _lastMessage);
                    _msgReceivedFlag.Release();                    
                }
                catch (Exception)
                {
                    if (_socket != null)
                    {
                        _socket.Dispose();
                        _socket = null;
                    }

                    DeviceDisconnected?.Invoke(this, _currentDevice);
                    _currentDevice = null;
                }
            }
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (_listenCancelTokenSource != null)
            {
                if (!_listenCancelTokenSource.IsCancellationRequested)
                {
                    _listenCancelTokenSource.Cancel();
                }
            }
        }

        public BTDevice CurrentDevice { get => _currentDevice; }

    }
}
