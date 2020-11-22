using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class IOConfigViewModel : AppViewModelBase
    {
        private String _deviceRepoId;
        private String _deviceId;
        private int _sendIndex;

        private BTDevice _currentDevice;
        private IBluetoothSerial _btSerial;

        System.Threading.SemaphoreSlim _recvSemephor;

        const string CRC_ERR_MSG_HDR = "IOCONFIG-RECV-CRC-ERR:";
        const string CRC_OK_MSG_HDR = "IOCONFIG-RECV-OK:";
        const string IOCONFIGRECVENDOK = "IOCONFIG-RECV-END:OK";
        const string IOCONFIGRECVENDFAIL = "IOCONFIG-RECV-END:FAIL";

        public const string DeviceId = "DEVICE_ID";
        public const string DeviceRepoId = "DEVICE_REPO_ID";

        public IOConfigViewModel()
        {
            _btSerial = SLWIOC.Create<IBluetoothSerial>();
            _btSerial.DeviceConnected += _btSerial_DeviceConnected;
            _btSerial.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;

            Config = null;

            WriteConfigurationCommand = new RelayCommand(async () => await WriteProfileAsync(), () => _btSerial.CurrentDevice != null);
            ResetConfigurationCommand = new RelayCommand(async () => await ResetConfigurationAsync(), () => _btSerial.CurrentDevice != null);
            RebootCommand = new RelayCommand(async () => await RebootAsync(), () => _btSerial.CurrentDevice != null);

        }

        private void _btSerial_DeviceDisconnected(object sender, BTDevice e)
        {
            WriteConfigurationCommand.RaiseCanExecuteChanged();
            ResetConfigurationCommand.RaiseCanExecuteChanged();
            RebootCommand.RaiseCanExecuteChanged();

            if (_currentDevice != null)
            {
                Popups.ShowAsync("Connection lost to device.");
            }
        }

        private void _btSerial_DeviceConnected(object sender, BTDevice e)
        {
            WriteConfigurationCommand.RaiseCanExecuteChanged();
            ResetConfigurationCommand.RaiseCanExecuteChanged();
            RebootCommand.RaiseCanExecuteChanged();
        }

        StringBuilder _builder = new StringBuilder();
        private async void _btSerial_ReceivedLine(object sender, string line)
        {
            Debug.WriteLine(line);
            if (line == IOCONFIGRECVENDOK)
            {
                await Popups.ShowAsync("SUCCESS", "Wrote configuration file.");
                await Storage.StoreAsync(Config, $"{_deviceId}.ioconfig.json");
            }
            else if (line == IOCONFIGRECVENDFAIL)
            {
                await Popups.ShowAsync("ERROR", "Could not write configuration file.");
            }
            else if (line.StartsWith(CRC_OK_MSG_HDR))
            {
                if (int.TryParse(line.Substring(CRC_OK_MSG_HDR.Length), out int lineNumber))

                    if (_recvSemephor != null && lineNumber == _sendIndex)
                    {
                        _recvSemephor.Release();
                    }
                _builder.Clear();

                Debug.WriteLine("CRC OK: " + lineNumber);
            }
            else if (line.StartsWith(CRC_ERR_MSG_HDR))
            {
                // if we get an error, we will automatically timeout.
                if (int.TryParse(line.Substring(CRC_ERR_MSG_HDR.Length), out int lineNumber))
                {
                    Debug.WriteLine("CRC Error Line: " + lineNumber);
                }
            }
            else
                foreach (var ch in line.ToCharArray())
                {
                    if (ch == '{')
                    {
                        _builder = new StringBuilder();
                        _builder.Append(ch);
                    }
                    else if (ch == '}')
                    {
                        Debug.WriteLine(_builder.ToString());
                        _builder.Append(ch);
                        Config = JsonConvert.DeserializeObject<IOConfig>(_builder.ToString());
                        BusyMessage = "Reading configuration";
                        IsBusy = false;
                        _builder.Clear();
                    }
                    else
                    {
                        _builder.Append(ch);
                    }
                }
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            _deviceRepoId = LaunchArgs.Parameters[IOConfigViewModel.DeviceRepoId].ToString();
            _deviceId = LaunchArgs.Parameters[IOConfigViewModel.DeviceId].ToString();

            if (!await Storage.HasKVPAsync(PairBTDeviceViewModel.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId)))
            {
                await Popups.ShowAsync("Must associate a BT Device first.");
                return;
            }

            var devices = await _btSerial.SearchAsync();
            var btDeviceId = await Storage.GetKVPAsync<string>(PairBTDeviceViewModel.ResolveBTDeviceIdKey(_deviceRepoId, _deviceId));

            var btDevice = devices.Where(bt => bt.DeviceId == btDeviceId).First();
            if (btDevice == null)
            {
                await Popups.ShowAsync("Could not find paired device.");
                return;
            }

            try
            {
                IsBusy = true;

                await _btSerial.ConnectAsync(btDevice);

                await _btSerial.SendAsync("HELLO\n");
                await Task.Delay(250);
                await _btSerial.SendAsync("PAUSE\n");
                await Task.Delay(250);

                await _btSerial.SendAsync("IOCONFIG-SEND\n");

                _currentDevice = btDevice;
            }
            catch (Exception)
            {
                await Popups.ShowAsync("Could not connect to device.");
            }
        }

        public override async Task IsClosingAsync()
        {
            if (_currentDevice != null)
            {
                // set to temporary value so we can check that it's no
                // longer current and not popup a device disconnected
                // message
                var device = _currentDevice;
                _currentDevice = null;
                try
                {
                    await _btSerial.SendAsync("CONTINUE\n");
                    await Task.Delay(500);
                    await _btSerial.DisconnectAsync(device);
                }
                catch (Exception) { }
            }

            await base.IsClosingAsync();
        }

        async Task WriteProfileAsync()
        {
            try
            {
                BusyMessage = "Writing Configuration.";
                IsBusy = true;

                await _btSerial.SendAsync("IOCONFIG-RECV-START\n");
                var chunkSize = 100;
                var json = JsonConvert.SerializeObject(Config);
                var remaining = json.Length;
                _sendIndex = 0;

                var retryCount = 0;

                while (remaining > 0)
                {
                    var start = _sendIndex * 100;
                    var end = Math.Min((start + chunkSize), json.Length);
                    var len = Math.Min(chunkSize, json.Length - start);
                    remaining = json.Length - end;
                    var jsonChunk = json.Substring(_sendIndex * 100, len);
                    byte crc = 0x00;
                    foreach (var ch in jsonChunk.ToCharArray())
                    {
                        crc += (byte)ch;
                    }

                    _recvSemephor = new System.Threading.SemaphoreSlim(0);
                    await _btSerial.SendAsync($"IOCONFIG-RECV:{_sendIndex:x2},{crc:x2},{jsonChunk}\n");
                    if (await _recvSemephor.WaitAsync(3000))
                    {
                        Debug.WriteLine("CONFIRMED: " + _sendIndex);
                        _sendIndex++;
                    }
                    else
                    {
                        Debug.WriteLine("TIMEOUT: " + _sendIndex);

                        retryCount++;
                    }
                }

                await _btSerial.SendAsync("IOCONFIG-RECV-END\n");

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
            }
        }

        async Task RebootAsync()
        {
            await _btSerial.SendAsync("REBOOT\n");
            await _btSerial.DisconnectAsync(_currentDevice);
        }


        async Task ResetConfigurationAsync()
        {
            await _btSerial.SendAsync("RESET-STATE\n");
        }

        private IOConfig _ioConfig;
        public IOConfig Config
        {
            get { return _ioConfig; }
            set { Set(ref _ioConfig, value); }
        }

        public RelayCommand WriteConfigurationCommand { get; private set; }
        public RelayCommand ResetConfigurationCommand { get; private set; }
        public RelayCommand RebootCommand { get; private set; }
    }
}
