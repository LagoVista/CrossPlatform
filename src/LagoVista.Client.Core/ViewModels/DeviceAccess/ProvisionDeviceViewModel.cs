using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class ProvisionDeviceViewModel : DeviceViewModelBase
    {
        private int _sendIndex;

        private BTDevice _currentDevice;
        private IBluetoothSerial _btSerial;

        System.Threading.SemaphoreSlim _recvSemephor;

        const string CRC_ERR_MSG_HDR = "SYSCONFIG-RECV-CRC-ERR:";
        const string CRC_OK_MSG_HDR = "SYSCONFIG-RECV-OK:";
        const string IOCONFIGRECVENDOK = "SYSCONFIG-RECV-END:OK";
        const string IOCONFIGRECVENDFAIL = "SYSCONFIG-RECV-END:FAIL";

        public ProvisionDeviceViewModel()
        {
            _btSerial = SLWIOC.Get<IBluetoothSerial>();
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
            if (line.StartsWith(CRC_OK_MSG_HDR))
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
                if (int.TryParse(line.Substring(CRC_ERR_MSG_HDR.Length), out int lineNumber))
                {
                    Debug.WriteLine("CRC Error Line: " + lineNumber);
                }
            }
            else if (line == IOCONFIGRECVENDOK)
            {
                await Popups.ShowAsync("SUCCESS", "Wrote configuration file.");
                await Storage.StoreAsync(Config, $"{DeviceId}.ioconfig.json");
            }
            else if (line == IOCONFIGRECVENDFAIL)
            {
                await Popups.ShowAsync("ERROR", "Could not write configuration file.");
            }
            else
            {
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

                        Config = JsonConvert.DeserializeObject<SysConfig>(_builder.ToString());
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
        }

        async Task WriteProfileAsync()
        {
            BusyMessage = "Writing Configuration.";
            IsBusy = true;

            await _btSerial.SendAsync("SYSCONFIG-RECV-START\n");
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
                await _btSerial.SendAsync($"SYSCONFIG-RECV:{_sendIndex:x2},{crc:x2},{jsonChunk}\n");
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

            await _btSerial.SendAsync("SYSCONFIG-RECV-END\n");

            IsBusy = false;
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

        public override async Task InitAsync()
        {
            await base.InitAsync();

            if (!await Storage.HasKVPAsync(PairBTDeviceViewModel.ResolveBTDeviceIdKey(DeviceRepoId, DeviceId)))
            {
                await Popups.ShowAsync("Must associate a BT Device first.");
                await CloseScreenAsync();
                return;
            }

            var devices = await _btSerial.SearchAsync();
            var btDeviceId = await Storage.GetKVPAsync<string>(PairBTDeviceViewModel.ResolveBTDeviceIdKey(DeviceRepoId, DeviceId));

            var btDevice = devices.Where(bt => bt.DeviceId == btDeviceId).First();
            if (btDevice == null)
            {
                await Popups.ShowAsync("Could not find paired device.");
                await CloseScreenAsync();
                return;
            }

            try
            {
                IsBusy = true;

                //await _btSerial.ConnectAsync(btDevice);
                await _btSerial.SendAsync("HELLO\n");
                await Task.Delay(250);
                await _btSerial.SendAsync("PAUSE\n");
                await Task.Delay(250);

                await _btSerial.SendAsync("SYSCONFIG-SEND\n");

                _currentDevice = btDevice;
            }
            catch (Exception)
            {
                await Popups.ShowAsync("Could not connect to device.");
            }
        }

        public async void Commission()
        {
            await _btSerial.SendAsync($"COMMISSION\n");
        }

        public ObservableCollection<BTDevice> ConnectedDevices { get; private set; } = new ObservableCollection<BTDevice>();

        public RelayCommand PairDeviceCommand { get; private set; }

        public override async Task IsClosingAsync()
        {
            if (_currentDevice != null)
            {
                try
                {
                    await _btSerial.SendAsync("CONTINUE\n");
                    await Task.Delay(1000);
                    await _btSerial.DisconnectAsync(_currentDevice);
                    _currentDevice = null;
                }
                catch (Exception)
                { }
            }

            await base.IsClosingAsync();
        }

        public RelayCommand CommissionCommand => new RelayCommand(Commission);

        private SysConfig _sysConfig;
        public SysConfig Config
        {
            get { return _sysConfig; }
            set { Set(ref _sysConfig, value); }
        }

        public RelayCommand WriteConfigurationCommand { get; private set; }
        public RelayCommand ResetConfigurationCommand { get; private set; }
        public RelayCommand RebootCommand { get; private set; }

    }
}