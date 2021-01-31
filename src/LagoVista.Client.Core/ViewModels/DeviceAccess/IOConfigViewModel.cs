using LagoVista.Client.Core.Models;
using LagoVista.Core.Commanding;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class IOConfigViewModel : DeviceViewModelBase
    {
        private int _sendIndex;

        System.Threading.SemaphoreSlim _recvSemephor;

        const string CRC_ERR_MSG_HDR = "IOCONFIG-RECV-CRC-ERR:";
        const string CRC_OK_MSG_HDR = "IOCONFIG-RECV-OK:";
        const string IOCONFIGRECVENDOK = "IOCONFIG-RECV-END:OK";
        const string IOCONFIGRECVENDFAIL = "IOCONFIG-RECV-END:FAIL";

        public IOConfigViewModel()
        {
            Config = null;

            WriteConfigurationCommand = new RelayCommand(async () => await WriteProfileAsync(), () => DeviceConnected);
            ResetConfigurationCommand = new RelayCommand(async () => await ResetConfigurationAsync(), () => DeviceConnected);
            RebootCommand = new RelayCommand(async () => await RebootAsync(), () => DeviceConnected);

        }

        protected override void OnBTSerial_DeviceDisconnected()
        {
            base.OnBTSerial_DeviceDisconnected();
            WriteConfigurationCommand.RaiseCanExecuteChanged();
            ResetConfigurationCommand.RaiseCanExecuteChanged();
            RebootCommand.RaiseCanExecuteChanged();
        }

        StringBuilder _builder = new StringBuilder();
        //private async void _btSerial_ReceivedLine(object sender, string line)
        protected override async void OnBTSerail_LineReceived(string line)
        {
            Debug.WriteLine(line);
            if (line == IOCONFIGRECVENDOK)
            {
                await Popups.ShowAsync("SUCCESS", "Wrote configuration file.");
                await Storage.StoreAsync(Config, $"{DeviceId}.ioconfig.json");
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

            try
            {
                IsBusy = true;

                if (!DeviceConnected)
                {
                    throw new InvalidOperationException("Not connected to bluetooth.");
                }

                await SendAsync("HELLO\n");
                await Task.Delay(250);
                await SendAsync("PAUSE\n");
                await Task.Delay(250);

                await SendAsync("IOCONFIG-SEND\n");
            }
            catch (Exception ex)
            {
                await Popups.ShowAsync($"Could not connect to device: {ex.Message}");
            }
        }

        public override async Task IsClosingAsync()
        {
            await SendAsync("CONTINUE\n");
            await base.IsClosingAsync();
        }

        async Task WriteProfileAsync()
        {
            try
            {
                BusyMessage = "Writing Configuration.";
                IsBusy = true;

                await SendAsync("IOCONFIG-RECV-START\n");
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
                    await SendAsync($"IOCONFIG-RECV:{_sendIndex:x2},{crc:x2},{jsonChunk}\n");
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

                await SendAsync("IOCONFIG-RECV-END\n");

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
            }
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
