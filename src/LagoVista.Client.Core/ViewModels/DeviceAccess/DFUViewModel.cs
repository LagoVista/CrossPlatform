using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Net;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.DeviceAdmin.Models;
using LagoVista.IoT.DeviceManagement.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DFUViewModel : DeviceViewModelBase
    {
        private string _firmwareUrl;

        private IBluetoothSerial _btSerial;

        public DFUViewModel()
        {
            UpdateDeviceFirmwareCommand = new LagoVista.Core.Commanding.RelayCommand(UpdateDeviceFirmware);
            CancelUpdatedCommand = new LagoVista.Core.Commanding.RelayCommand(CancelUpdate);
            _btSerial = SLWIOC.Create<IBluetoothSerial>();
            _btSerial.DeviceConnected += _btSerial_DeviceConnected;
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;
            _btSerial.DFUProgress += _btSerial_DFUProgress;
        }

        private void _btSerial_DFUProgress(object sender, DFUProgress progress)
        {
            DispatcherServices.Invoke(() =>
            {
                UploadProgress = (progress.Progress / 100.0f);
                StatusMessage = $"Progress {progress.Progress}% block {progress.BlockIndex} of {progress.TotalBlockCount} - checksum: {progress.CheckSum}";
            });
        }

        private void _btSerial_ReceivedLine(object sender, string e)
        {
            var variables = e.Split(',');
            foreach (var variable in variables)
            {
                Debug.WriteLine(variable);
                var parts = variable.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (key == "readonly-firmwareVersion")
                    {
                        DispatcherServices.Invoke(() =>
                        {
                            DeviceFirmwareVersion = value;
                        });
                    }

                    Debug.WriteLine($"{key}-{value}");
                }
            }
        }

        private async void _btSerial_DeviceConnected(object sender, BTDevice e)
        {
            await Popups.ShowAsync("Connection lost to device.");
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            if (!await Storage.HasKVPAsync(PairBTDeviceViewModel.ResolveBTDeviceIdKey(DeviceRepoId, DeviceId)))
            {
                await Popups.ShowAsync("Must associate a BT Device first.");
                return;
            }

            var devices = await _btSerial.SearchAsync();
            var btDeviceId = await Storage.GetKVPAsync<string>(PairBTDeviceViewModel.ResolveBTDeviceIdKey(DeviceRepoId, DeviceId));

            var btDevice = devices.Where(bt => bt.DeviceId == btDeviceId).First();
            if (btDevice == null)
            {
                await Popups.ShowAsync("Could not find paired device.");
                return;
            }

            if(!_btSerial.IsConnected)
            {
                await Popups.ShowAsync("Device not connected.");
                return;
            }

            try
            {
                await _btSerial.ConnectAsync(btDevice);

                await _btSerial.SendAsync("HELLO\n");
                await Task.Delay(250);
                await _btSerial.SendAsync("PAUSE\n");
                await Task.Delay(250);
                await _btSerial.SendAsync("PROPERTIES\n");

                var result = await PerformNetworkOperation(async () =>
                {
                    var getDeviceUri = $"/api/device/{DeviceRepoId}/{DeviceId}";

                    var restClient = new FormRestClient<Device>(base.RestClient);

                    var device = await restClient.GetAsync(getDeviceUri);
                    var deviceType = await this.RestClient.GetAsync<DetailResponse<DeviceType>>($"/api/devicetype/{device.Result.Model.DeviceType.Id}");
                    if (EntityHeader.IsNullOrEmpty(deviceType.Result.Model.Firmware))
                    {
                        throw new Exception($"No firmware found for device type {deviceType.Result.Model.Name}");
                    }

                    var firmware = await this.RestClient.GetAsync<DetailResponse<Firmware>>($"/api/firmware/{deviceType.Result.Model.Firmware.Id}");
                    _firmwareUrl = $"/api/firmware/download/{firmware.Result.Model.Id}/{firmware.Result.Model.DefaultRevision.Id}";

                    HasServerFirmware = true;
                    FirmwareName = firmware.Result.Model.Name;
                    FirmwareSKU = firmware.Result.Model.FirmwareSku;
                    ServerFirmwareVersion = firmware.Result.Model.DefaultRevision.Text;
                });

                if (!result)
                {
                    await Popups.ShowAsync("Could not connect to server.");
                    this.CloseScreen();
                }
            }
            catch (Exception)
            {
                await Popups.ShowAsync("Could not connect to device.");
                this.CloseScreen();
            }

        }

        private void CancelUpdate()
        {

        }



        public async void UpdateDeviceFirmware()
        {
            StatusMessage = "Downloading firmare.";
            var responseMessage = await this.HttpClient.GetAsync(_firmwareUrl);
            var buffer = await responseMessage.Content.ReadAsByteArrayAsync();
            StatusMessage = "Firmware Downloading starting device firmware update.";
            await _btSerial.SendAsync("READFIRMWARE\n");
            await _btSerial.SendDFUAsync(buffer);
        }

        public LagoVista.Core.Commanding.RelayCommand UpdateDeviceFirmwareCommand { get; private set; }
        public LagoVista.Core.Commanding.RelayCommand CancelUpdatedCommand { get; private set; }

        private string _deviceFirmwareVersion;
        public string DeviceFirmwareVersion
        {
            get { return _deviceFirmwareVersion; }
            set { Set(ref _deviceFirmwareVersion, value); }
        }

        private string _serverFirmwareVersion;
        public string ServerFirmwareVersion
        {
            get { return _serverFirmwareVersion; }
            set { Set(ref _serverFirmwareVersion, value); }
        }

        private string _firmareName;
        public string FirmwareName
        {
            get { return _firmareName; }
            set { Set(ref _firmareName, value); }
        }

        private string _firmwareSKU;
        public string FirmwareSKU
        {
            get { return _firmwareSKU; }
            set { Set(ref _firmwareSKU, value); }
        }

        private float _uploadProgress;
        public float UploadProgress
        {
            get { return _uploadProgress; }
            set { Set(ref _uploadProgress, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        private bool _hasServerFirmware = false;
        public bool HasServerFirmware
        {
            get { return _hasServerFirmware; }
            set { Set(ref _hasServerFirmware, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { Set(ref _statusMessage, value); }
        }
    }
}
