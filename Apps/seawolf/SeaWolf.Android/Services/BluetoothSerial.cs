using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System;
using Android.Bluetooth;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Java.IO;
using LagoVista.Core.PlatformSupport;
using Android.Content;
using Android.App;
using Android.Util;
using System.Linq;
using System.Collections.Generic;
using Java.Util;
using System.Threading;
using System.Text;

namespace LagoVista.XPlat.Droid.Services
{
    public class BluetoothSerial : IBluetoothSerial
    {
        ILogger _logger;

        public enum BluetoothSerialState
        {
            None,
            Error,
            Connecting,
            Connected,
            Disconnecting,
            Disconnected,
        }

        public BluetoothSerialState State { get; private set; }

        private readonly IPopupServices _popups;
        const int READ_BUFFER_SIZE = 1024;

        private string _lastMessage;
        private BluetoothSocket _bluetoothSocket;
        private System.IO.Stream _inputStream;
        private System.IO.Stream _outputStream;

        public BTDevice CurrentDevice { get; private set; }

        public bool IsConnected => CurrentDevice != null;

        private CancellationTokenSource _listenCancelTokenSource = new CancellationTokenSource();
        private SemaphoreSlim _msgReceivedFlag = new SemaphoreSlim(0);

        public event EventHandler<string> ReceivedLine;
        public event EventHandler<DFUProgress> DFUProgress;
        public event EventHandler DFUCompleted;
        public event EventHandler<string> DFUFailed;
        public event EventHandler<BTDevice> DeviceDiscovered;
        public event EventHandler<BTDevice> DeviceConnected;
        public event EventHandler<BTDevice> DeviceConnecting;
        public event EventHandler<BTDevice> DeviceDisconnected;

        private readonly Android.Content.Context _context;
        private readonly DeviceDiscoveredReceiver _receiver;
        BluetoothAdapter _bluetoothAdapter;

        UUID _myUUID;

        // Abstract BT devices to be used by the UI/
        private readonly ObservableCollection<BTDevice> _btDevices;

        // BT Devices that are android objects, used to connect.
        private readonly List<BluetoothDevice> _bluetoothDevices;

        public BluetoothSerial(ILogger logger, Android.Content.Context context, IPopupServices popups)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _popups = popups ?? throw new ArgumentNullException(nameof(context));

            _receiver = new DeviceDiscoveredReceiver();
            _receiver.DeviceDiscovered += _receiver_DeviceDiscovered;
            _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            _btDevices = new ObservableCollection<BTDevice>();
            _bluetoothDevices = new List<BluetoothDevice>();
        }

        private void _receiver_DeviceDiscovered(object sender, BluetoothDevice device)
        {
            _bluetoothDevices.Add(device);

            Log.Warn("NuvIoT - Bluetooth", $"Found Bluetooth device: {device.Name}/{device.Address}");

            if (!_btDevices.Where(bt => bt.DeviceId == device.Address).Any())
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    var btDevice = new BTDevice() { DeviceId = device.Address, DeviceName = device.Name };
                    this.DeviceDiscovered?.Invoke(this, btDevice);
                    _btDevices.Add(btDevice);
                });
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

        private void SetState(BluetoothSerialState state)
        {
            State = state;
        }

        private async Task<String> WaitForResponseAsync()
        {
            if (CurrentDevice == null)
            {
                throw new InvalidOperationException("No connected.");
            }

            _lastMessage = null;

            await _msgReceivedFlag.WaitAsync(2500);
            if (!String.IsNullOrEmpty(_lastMessage) && _lastMessage.Contains("fail"))
            {
                throw new Exception(_lastMessage);
            }

            return _lastMessage;
        }


        public async Task ConnectAsync(BTDevice device)
        {
            DeviceConnecting?.Invoke(this, device);
            SetState(BluetoothSerialState.Connecting);
            var androidBluetoothDevice = _bluetoothDevices.Where(bt => bt.Address == device.DeviceId).SingleOrDefault();

            if (androidBluetoothDevice == null)
            {
                SetState(BluetoothSerialState.Error);
                throw new Exception($"Could not find bluetooth device {device.DeviceName} with address {device.DeviceId} ");
            }

            _myUUID = UUID.RandomUUID();

            var attempt = 0;
            while (attempt++ < 5)
            {
                try
                {
                    var serialUUID = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
                    var socket = androidBluetoothDevice.CreateRfcommSocketToServiceRecord(serialUUID);
                    await socket.ConnectAsync();
                    _inputStream = socket.InputStream;
                    _outputStream = socket.OutputStream;
                    _bluetoothSocket = socket;

                    SetState(BluetoothSerialState.Connected);
                    StartListening();
                    DeviceConnected?.Invoke(this, device);
                    CurrentDevice = device;
                }
                catch (IOException ex)
                {
                    await _popups.ShowAsync($"Could not create Bluetooth Socket, Attempt {attempt} of 5");
                    ShutDown(ex);
                }
            }
        }

        public Task DisconnectAsync(BTDevice deviceId)
        {
            CancelReadTask();
            ShutDown();
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<ObservableCollection<BTDevice>> SearchAsync()
        {
            _btDevices.Clear();
            _bluetoothDevices.Clear();

            var filter = new IntentFilter(BluetoothDevice.ActionFound);
            _context.RegisterReceiver(_receiver, filter);

            if (!_bluetoothAdapter.IsEnabled)
            {
                await _popups.ShowAsync("Bluetooth is not enabled on your device, please ensure it is turned on and you have granted the application permission to use Bluetooth.");
            }
            else
            {
                if (!_bluetoothAdapter.StartDiscovery())
                {
                    await _popups.ShowAsync("Could not start bluetooth discovery, please restart your application and try again.");
                }
            }

            await Task.Delay(2500);

            return _btDevices;
        }

        private void ShutDown(Exception ex = null)
        {
            lock (this)
            {
                if (_outputStream != null)
                {
                    _outputStream.Dispose();
                    _outputStream = null;
                }

                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }

                if (_bluetoothSocket != null)
                {
                    _bluetoothSocket.Dispose();
                    _bluetoothSocket = null;
                }

                if (CurrentDevice != null)
                {
                    DeviceDisconnected(this, CurrentDevice);
                    CurrentDevice = null;
                }
            }

            if (ex != null)
            {
                _logger.AddException("BluetoothSerial_ConnectAsync", ex);
                Log.Debug("NuvIoT - BluetoothSerial", "EXCEPTION: " + ex.Message);
                Log.Debug("NuvIoT - BluetoothSerial", "EXCEPTION: " + ex.GetType());
                Log.Debug("NuvIoT - BluetoothSerial", ex.StackTrace);
            }
        }

        public async Task SendAsync(string msg)
        {
            try
            {
                if (CurrentDevice == null)
                {
                    throw new ArgumentNullException(nameof(CurrentDevice));
                }

                var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);

                await _outputStream.WriteAsync(buffer, 0, buffer.Length);
                await _outputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                ShutDown(ex);
                throw;
            }
        }

        public async Task SendDFUAsync(byte[] firmware)
        {
            if (CurrentDevice == null)
            {
                throw new InvalidOperationException("No connected.");
            }

            try
            {
                await SendAsync("FIRMWARE\n");

                var response = await WaitForResponseAsync();
                int retryCount = 0;
                while ((response == null || response.Trim() != "ok-start;") && retryCount++ < 5)
                {
                    response = await WaitForResponseAsync();
                    Log.Debug("NuvIoT - BluetoothSerial", "INCORRECT RESPONSE, EXPECTING ok-start : " + response);
                }

                if (retryCount == 5)
                {
                    throw new Exception("Timeout waiting for ack from device start dfu.");
                }

                var length = firmware.Length;


                _outputStream.WriteByte((byte)(length >> 24));
                _outputStream.WriteByte((byte)(length >> 16));
                _outputStream.WriteByte((byte)(length >> 8));
                _outputStream.WriteByte((byte)(length & 0xFF));
                await _outputStream.FlushAsync();

                response = await WaitForResponseAsync();
                if (String.IsNullOrEmpty(response))
                {
                    throw new Exception("Timeout waiting for ack from dfu size.");
                }

                int blockSize = 500;

                short blocks = (short)((firmware.Length / blockSize) + 1);

                _outputStream.WriteByte((byte)(blocks >> 8));
                _outputStream.WriteByte((byte)(blocks & 0xFF));
                await _outputStream.FlushAsync();

                response = await WaitForResponseAsync();
                if (String.IsNullOrEmpty(response))
                {
                    throw new Exception("Timeout waiting for ack from dfu size.");
                }

                for (var idx = 0; idx < blocks; ++idx)
                {
                    var start = idx * blockSize;
                    var len = firmware.Length - start;

                    len = Math.Min(blockSize, len);

                    _outputStream.WriteByte((byte)(len >> 8));
                    _outputStream.WriteByte((byte)(len & 0xFF));
                    await _outputStream.FlushAsync();

                    var sendBuffer = new byte[len];
                    Array.Copy(firmware, start, sendBuffer, 0, len);

                    // Send check sum
                    byte checkSum = 0;
                    for (var ch = 0; ch < len; ch++)
                    {
                        checkSum += sendBuffer[ch];
                    }

                    _outputStream.Write(sendBuffer, 0, len);
                    await _outputStream.FlushAsync();

                    _outputStream.WriteByte((byte)(checkSum));
                    await _outputStream.FlushAsync();
                    await _outputStream.FlushAsync();

                    var blockResponse = await WaitForResponseAsync();
                    if (String.IsNullOrEmpty(blockResponse))
                    {
                        throw new Exception("Timeout waiting for block response.");
                    }

                    DFUProgress?.Invoke(this, new DFUProgress()
                    {
                        Progress = (idx * 100) / blocks,
                        BlockIndex = idx,
                        TotalBlockCount = blocks,
                        CheckSum = checkSum
                    });
                }

                DFUCompleted?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                DFUFailed?.Invoke(this, ex.Message);
                ShutDown(ex);
            }
        }
        
        /// <summary>
        /// - Create a DataReader object
        /// - Create an async task to read from the SerialDevice InputStream
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartListening()
        {
            Task.Run(() =>
            {
                Log.Debug("NuvIoT - BluetoothSerial", "BluetoothSerial_Listen - Starting.");
                _listenCancelTokenSource = new CancellationTokenSource();

                byte[] buffer = new byte[READ_BUFFER_SIZE];  // buffer store for the stream
                StringBuilder readMessage = new StringBuilder();

                var running = true;
                while (running)
                {
                    try
                    {
                        var bytesRead = _inputStream.Read(buffer, 0, READ_BUFFER_SIZE);
                        var msg = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead);
                        readMessage.Append(msg);
                        var lines = msg.Split("\n");
                        foreach(var line in lines)
                        {
                            if (!String.IsNullOrEmpty(line.Trim()))
                            {
                                this.ReceivedLine?.Invoke(this, line);
                                if (line != null && line.StartsWith("fwupdate="))
                                {
                                    _lastMessage = line.Substring("fwupdate=".Length);
                                    _msgReceivedFlag.Release();
                                }
                                Log.Debug("NuvIoT - BluetoothSerial", $"Input Message > {line}");
                            }
                        }                        
                    }
                    catch (Exception ex)
                    {
                        running = false;
                        ShutDown(ex);                        
                    }
                }
            });
        }

        public Task StopSearchingAsync()
        {
            _context.UnregisterReceiver(_receiver);
            return Task.CompletedTask;
        }

        class DeviceDiscoveredReceiver : BroadcastReceiver
        {
            public event EventHandler<BluetoothDevice> DeviceDiscovered;

            public override void OnReceive(Context context, Intent intent)
            {
                var action = intent.Action;
                if (BluetoothDevice.ActionFound.Equals(action))
                {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    if (!String.IsNullOrEmpty(device.Name))
                    {
                        DeviceDiscovered?.Invoke(this, device);
                    }
                    else
                    {
                        Log.Warn("NuvIoT - Bluetooth", $"Found Bluetooth device without name: {device.Address}");
                    }
                }
            }
        }
    }
}