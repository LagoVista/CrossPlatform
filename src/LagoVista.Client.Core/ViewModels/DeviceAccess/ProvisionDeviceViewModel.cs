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


        System.Threading.SemaphoreSlim _recvSemephor;

        const string CRC_ERR_MSG_HDR = "SYSCONFIG-RECV-CRC-ERR:";
        const string CRC_OK_MSG_HDR = "SYSCONFIG-RECV-OK:";
        const string IOCONFIGRECVENDOK = "SYSCONFIG-RECV-END:OK";
        const string IOCONFIGRECVENDFAIL = "SYSCONFIG-RECV-END:FAIL";

        public ProvisionDeviceViewModel()
        {
            Config = null;

            WriteConfigurationCommand = new RelayCommand(async () => await WriteProfileAsync(), () => DeviceConnected);
            ResetConfigurationCommand = new RelayCommand(async () => await ResetConfigurationAsync(), () => DeviceConnected);
            RebootCommand = new RelayCommand(async () => await RebootAsync(), () => DeviceConnected);
        }

        protected override void OnBLEDevice_Disconnected(BLEDevice device)
        {
            WriteConfigurationCommand.RaiseCanExecuteChanged();
            ResetConfigurationCommand.RaiseCanExecuteChanged();
            RebootCommand.RaiseCanExecuteChanged();
        }

        StringBuilder _builder = new StringBuilder();
        //   private async void _btSerial_ReceivedLine(object sender, string line)

        protected async override void OnBTSerail_MsgReceived(string line)
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

            await SendAsync("SYSCONFIG-RECV-START\n");
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
                await SendAsync($"SYSCONFIG-RECV:{_sendIndex:x2},{crc:x2},{jsonChunk}\n");
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

            await SendAsync("SYSCONFIG-RECV-END\n");

            IsBusy = false;
        }

        async Task RebootAsync()
        {
            await SendAsync("REBOOT\n");
            await DisconnectAsync();
        }

        async Task ResetConfigurationAsync()
        {
            await SendAsync("RESET-STATE\n");
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            //await _btSerial.ConnectAsync(btDevice);
            await SendAsync("HELLO\n");
            await Task.Delay(250);
            await SendAsync("PAUSE\n");
            await Task.Delay(250);

            await SendAsync("SYSCONFIG-SEND\n");
        }

        public async void Commission()
        {
            await SendAsync($"COMMISSION\n");
            await DisconnectAsync();
        }

        public ObservableCollection<BTDevice> ConnectedDevices { get; private set; } = new ObservableCollection<BTDevice>();

        public RelayCommand PairDeviceCommand { get; private set; }

        public override async Task IsClosingAsync()
        {
            await SendAsync("CONTINUE\n");
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