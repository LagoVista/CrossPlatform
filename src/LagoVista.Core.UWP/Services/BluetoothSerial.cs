using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LagoVista.Core.UWP.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        private RfcommDeviceService _service;
        private StreamSocket _socket;
        private DataWriter dataWriterObject;
        private DataReader dataReaderObject;

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
            DeviceConnecting?.Invoke(this, device);

            _service = await RfcommDeviceService.FromIdAsync(device.DeviceId);

            _socket = new StreamSocket();

            await _socket.ConnectAsync(_service.ConnectionHostName, _service.ConnectionServiceName);
            dataWriterObject = new DataWriter(_socket.OutputStream);
            Listen();

            DeviceConnected?.Invoke(this, device);

            _currentDevice = device;
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
            await _msgReceivedFlag.WaitAsync();
            return _lastMessage;
        }

        public async Task SendDFUAsync(BTDevice device, byte[] firmware)
        {
            try
            {
                await SendLineAsync("FIRMWARE\n");

                var response = await WaitForResponseAsync();
                while (response.Trim() != "ok-start")
                {
                    response = await WaitForResponseAsync();
                }

                int blockSize = 500;

                short blocks = (short)((firmware.Length / blockSize) + 1);

                dataWriterObject.WriteByte((byte)(blocks >> 8));
                dataWriterObject.WriteByte((byte)(blocks & 0xFF));
                var storeResult = await dataWriterObject.StoreAsync();

                response = await WaitForResponseAsync();

                for (var idx = 0; idx < blocks; ++idx)
                {
                    var start = idx * blockSize;
                    var len = firmware.Length - start;

                    len = Math.Min(blockSize, len);

                    dataWriterObject.WriteByte((byte)(len >> 8));
                    dataWriterObject.WriteByte((byte)(len & 0xFF));
                    storeResult = await dataWriterObject.StoreAsync();

                    var sendBuffer = new byte[len];
                    Array.Copy(firmware, start, sendBuffer, 0, len);

                    // Send check sum
                    byte checkSum = 0;
                    for (int ch = 0; ch < len; ch++)
                    {
                        checkSum += sendBuffer[ch];
                    }

                    dataWriterObject.WriteBytes(sendBuffer);
                    storeResult = await dataWriterObject.StoreAsync();

                    dataWriterObject.WriteByte((byte)(checkSum));
                    storeResult = await dataWriterObject.StoreAsync();

                    var blockResponse = await WaitForResponseAsync();

                    DFUProgress?.Invoke(this, (idx * 100) / blocks);
                }

                DFUCompleted?.Invoke(this, null);
            }
            catch(Exception ex)
            {
                _socket.Dispose();
                _socket = null;
                DFUFailed?.Invoke(this, ex.Message);
                DeviceDisconnected?.Invoke(this, _currentDevice);
                _currentDevice = null;
            }
        }

        public async Task SendLineAsync(string msg)
        {
            try
            {
                dataWriterObject.WriteString(msg);

                // Launch an async task to complete the write operation
                await dataWriterObject.StoreAsync();
            }
            catch(Exception)
            {
                _socket.Dispose();
                _socket = null;
                DeviceDisconnected?.Invoke(this, _currentDevice);
                _currentDevice = null;
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
                    dataReaderObject = new DataReader(_socket.InputStream);
                    // keep reading the serial input
                    while (true && _socket != null)
                    {
                        await ReadAsync(_listenCancelTokenSource.Token);
                    }
                }
            }
            catch (Exception)
            {
                _socket.Dispose();
                _socket = null;
                DeviceDisconnected?.Invoke(this, _currentDevice);
                _currentDevice = null;
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            cancellationToken.ThrowIfCancellationRequested();

            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                try
                {
                    _lastMessage = dataReaderObject.ReadString(bytesRead);
                    this.ReceivedLine?.Invoke(this, _lastMessage);
                    _msgReceivedFlag.Release();
                    Debug.WriteLine(_lastMessage);
                }
                catch (Exception)
                {
                    _socket.Dispose();
                    _socket = null;
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


    }
}
