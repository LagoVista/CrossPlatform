using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Net;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Geo;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.DeviceManagement.Core.Models;
using LagoVista.IoT.DeviceManagement.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public abstract class DeviceViewModelBase : AppViewModelBase
    {
        private String _deviceRepoId;
        private String _deviceId;
        private bool _isNotFirstVessel;
        private bool _isNotLastVessel;
        String _systemStatus = "All systems nominal";
        GeoLocation _currentVeseelLocation;
        ObservableCollection<DeviceSummary> _userDevices;

        Uri _wsUri;
        IWebSocket _webSocket;

        private readonly IGATTConnection _gattConnection;
        public IGATTConnection GattConnection => _gattConnection;

        private BLEDevice _bleDevice;

        public const string DEVICE_ID = "DEVICE_ID";
        public const string DEVICE_REPO_ID = "DEVICE_REPO_ID";
        public const string BT_DEVICE_ADDRESS = "BT_ADDRESS";

        public DeviceViewModelBase()
        {
            if (SLWIOC.Contains(typeof(IGATTConnection)))
            {
                _gattConnection = SLWIOC.Get<IGATTConnection>();
            }

            NextVesselCommand = new RelayCommand(NextVessel);
            PreviousVesselCommand = new RelayCommand(PreviousVessel);
        }

        public async Task SubscribeToWebSocketAsync()
        {
            var callResult = await PerformNetworkOperation(async () =>
            {
                if (_webSocket != null)
                {
                    await _webSocket.CloseAsync();
                    Debug.WriteLine("Web Socket is Closed.");
                    _webSocket = null;
                }

                var channelId = GetChannelURI();
                var wsResult = await RestClient.GetAsync<InvokeResult<string>>(channelId);
                if (wsResult.Successful)
                {
                    var url = wsResult.Result.Result;
                    Debug.WriteLine(url);
                    _wsUri = new Uri(url);

                    _webSocket = SLWIOC.Create<IWebSocket>();
                    _webSocket.MessageReceived += _webSocket_MessageReceived;
                    var wsOpenResult = await _webSocket.OpenAsync(_wsUri);
                    if (wsOpenResult.Successful)
                    {
                        Debug.WriteLine("OPENED CHANNEL");
                    }
                    return wsOpenResult;
                }
                else
                {
                    return wsResult.ToInvokeResult();
                }
            });
        }

        private async Task InitBLEAsync()
        {
            if (_gattConnection != null)
            {
                _gattConnection.DeviceConnected += BtSerial_DeviceConnected;
                _gattConnection.DeviceDiscovered += _gattConnection_DeviceDiscovered;
                _gattConnection.DeviceDisconnected += BtSerial_DeviceDisconnected;
                _gattConnection.ReceiveConsoleOut += BtSerial_ReceivedLine;
                _gattConnection.DFUProgress += BtSerial_DFUProgres;
                _gattConnection.DFUFailed += BtSerial_DFUFailed;
                _gattConnection.CharacteristicChanged += _gattConnection_CharacteristicChanged;
                _gattConnection.DFUCompleted += BtSerial_DFUCompleted;

                if (CurrentDevice != null)
                {
                    BLEDevice = _gattConnection.ConnectedDevices.Where(dvc => dvc.DeviceAddress == CurrentDevice.MacAddress).FirstOrDefault();
                    if (BLEDevice == null)
                    {
                        await GattConnection.StartScanAsync();
                    }
                }
                else
                {
                    BLEDevice = null;
                }
            }

        }

        public async override Task InitAsync()
        {
            if (LaunchArgs.Parameters.ContainsKey(nameof(Device)))
            {
                CurrentDevice = GetLaunchArg<Device>(nameof(Device));
            }
            else
            {
                CurrentDevice = null;
            }

            await InitBLEAsync();

            await base.InitAsync();
        }

        private async void _gattConnection_DeviceDiscovered(object sender, BLEDevice e)
        {
            if (CurrentDevice != null && e.DeviceAddress == CurrentDevice.MacAddress)
            {
                lock (this)
                {
                    if (_isConnecting)
                    {
                        return;
                    }

                    _isConnecting = true;
                }

                await GattConnection.StopScanAsync();
                await _gattConnection.ConnectAsync(e);
            }
        }

        public async override Task ReloadedAsync()
        {
            await InitBLEAsync();
            await base.ReloadedAsync();
        }

        public async override Task IsClosingAsync()
        {
            await base.IsClosingAsync();

            _gattConnection.DeviceConnected -= BtSerial_DeviceConnected;
            _gattConnection.DeviceDisconnected -= BtSerial_DeviceDisconnected;
            _gattConnection.DeviceDiscovered -= _gattConnection_DeviceDiscovered;
            _gattConnection.ReceiveConsoleOut -= BtSerial_ReceivedLine;
            _gattConnection.CharacteristicChanged -= _gattConnection_CharacteristicChanged;
            _gattConnection.DFUProgress -= BtSerial_DFUProgres;
            _gattConnection.DFUFailed -= BtSerial_DFUFailed;
            _gattConnection.DFUCompleted -= BtSerial_DFUCompleted;
            await _gattConnection.StopScanAsync();

            if (_webSocket != null)
            {
                await _webSocket.CloseAsync();
                Debug.WriteLine("Web Socket is Closed.");
                _webSocket = null;
            }
        }

        private void _gattConnection_CharacteristicChanged(object sender, BLECharacteristicsValue e)
        {
            Debug.WriteLine(e.Uid + " " + e.Value);
        }

        #region DFU
        protected virtual void OnDFUCompleted() { }
        protected virtual void OnDFUFailed(String err) { }
        protected virtual void OnDFUProgress(Models.DFUProgress e) { }


        private void BtSerial_DFUCompleted(object sender, EventArgs e)
        {
            OnDFUCompleted();
        }

        private void BtSerial_DFUFailed(object sender, string e)
        {
            OnDFUFailed(e);
        }

        private void BtSerial_DFUProgres(object sender, Models.DFUProgress e)
        {
            OnDFUProgress(e);
        }

        private void BtSerial_ReceivedLine(object sender, string e)
        {
            OnBTSerail_MsgReceived(e);

            var lines = e.Split('\n');
            foreach (var line in lines)
            {
                OnBTSerail_LineReceived(line);
            }
        }

        private bool _isConnecting = false;

        private async void BtSerial_DeviceDisconnected(object sender, Models.BLEDevice e)
        {
            if (e.DeviceAddress == CurrentDevice.MacAddress)
            {
                BLEDevice = null;
                OnBLEDevice_Disconnected(e);
                RaisePropertyChanged(nameof(DeviceConnected));
                await GattConnection.StartScanAsync();
            }
        }

        private async void BtSerial_DeviceConnected(object sender, Models.BLEDevice e)
        {
            _isConnecting = false;

            if (e.DeviceAddress == CurrentDevice.MacAddress)
            {
                BLEDevice = e;
                OnBLEDevice_Connected(e);
                RaisePropertyChanged(nameof(DeviceConnected));

                var service = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Find(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
                var characteristics = service.Characteristics.Find(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_STATE);


                await GattConnection.SubscribeAsync(e, service, characteristics);
            }
        }
        #endregion

        public async void NextVessel()
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));
            deviceIdx++;

            if (deviceIdx < UserDevices.Count)
            {
                DeviceId = UserDevices[deviceIdx].Id;
                await LoadDevice();
            }
        }

        public async void PreviousVessel()
        {
            var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));
            deviceIdx--;

            if (deviceIdx > -1)
            {
                DeviceId = UserDevices[deviceIdx].Id;
                await LoadDevice();
            }
        }


        public String DeviceId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceId) && LaunchArgs.Parameters.ContainsKey(DEVICE_ID))
                {
                    _deviceId = LaunchArgs.Parameters[DEVICE_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModelBase.DeviceId));
                }

                return _deviceId;
            }
            set
            {
                _deviceId = value;
            }
        }

        bool _isBLEConnected = false;
        public bool IsBLEConnected
        {
            get => _isBLEConnected;
            set
            {
                Set(ref _isBLEConnected, value);
                RaisePropertyChanged(nameof(IsBLEDisconnected));
            }
        }

        public bool IsBLEDisconnected
        {
            get => !_isBLEConnected;
        }

        bool _isDeviceConnectedToServer = false;
        public bool IsDeviceConnectedToServer
        {
            get => _isDeviceConnectedToServer;
            set
            {
                Set(ref _isDeviceConnectedToServer, value);
                RaisePropertyChanged(nameof(IsDeviceDisconnectedToServer));
            }
        }

        public bool IsDeviceDisconnectedToServer
        {
            get => !_isDeviceConnectedToServer;
        }

        public BLEDevice BLEDevice
        {
            get => _bleDevice;
            set
            {
                Set(ref _bleDevice, value);
                IsBLEConnected = value != null;
            }
        }

        private async void CurrentDeviceChanged()
        {
            if (BLEDevice != null)
            {
                await _gattConnection.DisconnectAsync(BLEDevice);
            }

            if (CurrentDevice != null)
            {
                await _gattConnection.StartScanAsync();
            }
        }

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get => _currentDevice;
            set
            {
                if (value != _currentDevice)
                {
                    _currentDevice = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected KeyValuePair<string, object> DeviceLaunchArgsParam => new KeyValuePair<string, object>(nameof(Device), CurrentDevice);

        protected Task SendAsync(String msg)
        {
            return Task.CompletedTask;
        }

        protected Task DisconnectAsync()
        {
            return _gattConnection.DisconnectAsync(_bleDevice);
        }

        protected bool DeviceConnected
        {
            get { return _bleDevice != null && _bleDevice.Connected; }
        }

        protected virtual void OnBTSerail_MsgReceived(string msg) { }
        protected virtual void OnBTSerail_LineReceived(string line) { }
        protected virtual void OnBLEDevice_Connected(BLEDevice device) { }
        protected virtual void OnBLEDevice_Disconnected(BLEDevice device) { }

        protected virtual void SendDFU(byte[] buffer)
        {
            //_gattConnection.SendDFUAsync(buffer);
        }

        // Bit of a hack, if we are going from a device view to a child view
        // we don't want to disconnect so we keep the BT connection alive.
        // set a nasty flag to determine if this is the case.
        private bool _isShowingNewView = false;

        public async void DisconnectBTDevice()
        {
            if (!_isShowingNewView && _bleDevice.Connected)
            {
                await _gattConnection.DisconnectAsync(_bleDevice);
            }

            _isShowingNewView = false;
        }

        protected string DeviceRepoId
        {
            get
            {
                if (String.IsNullOrEmpty(_deviceRepoId))
                {
                    _deviceRepoId = LaunchArgs.Parameters[DEVICE_REPO_ID].ToString() ?? throw new ArgumentNullException(nameof(DeviceViewModelBase.DeviceRepoId));
                }

                return _deviceRepoId;
            }
        }

        protected async Task<InvokeResult> LoadDevice()
        {
            return await PerformNetworkOperation(async () =>
            {
                await GattConnection.StopScanAsync();

                CurrentDevice = null;
                var deviceResponse = await DeviceManagementClient.GetDeviceAsync(AppConfig.DeviceRepoId, DeviceId);
                if (deviceResponse.Successful)
                {
                    CurrentDevice = deviceResponse.Model;
                    var deviceIdx = UserDevices.IndexOf(UserDevices.FirstOrDefault(dev => dev.Id == CurrentDevice.Id));

                    IsNotLastVessel = deviceIdx < UserDevices.Count - 1;
                    IsNotFirstVessel = deviceIdx > 0;

                    if (CurrentDevice.GeoLocation != null)
                    {
                        CurrentVeseelLocation = CurrentDevice.GeoLocation;
                    }
                    else
                    {
                        /*                        var lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
                                                if (lastKnownLocation != null)
                                                {
                                                    CurrentVeseelLocation = new GeoLocation(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
                                                }*/
                    }

                    GeoFences.Clear();
                    foreach (var geoFence in CurrentDevice.GeoFences)
                    {
                        GeoFences.Add(geoFence);
                    }
                }

                if (!String.IsNullOrEmpty(CurrentDevice.MacAddress))
                {
                    var bleDevice = _gattConnection.DiscoveredDevices.FirstOrDefault(ble => ble.DeviceAddress == CurrentDevice.MacAddress);
                    if (bleDevice != null)
                    {
                        await _gattConnection.ConnectAsync(bleDevice);
                    }
                    else
                    {
                        await _gattConnection.StartScanAsync();
                    }
                }
                else
                {
                    await _gattConnection.StartScanAsync();
                }

                return deviceResponse.ToInvokeResult();
            });
        }

        protected async Task LoadUserDevices()
        {
            await PerformNetworkOperation(async () =>
            {
                var path = $"/api/devices/{AppConfig.DeviceRepoId}/{this.AuthManager.User.Id}";

                ListRestClient<DeviceSummary> _formRestClient = new ListRestClient<DeviceSummary>(RestClient);
                var result = await _formRestClient.GetForOrgAsync(path);
                if (!result.Successful)
                {
                    return result.ToInvokeResult();
                }

                UserDevices = new ObservableCollection<DeviceSummary>(result.Model);

                if (UserDevices.Count > 0)
                {
                    HasDevices = true;
                    NoDevices = !HasDevices;
                    DeviceId = await Storage.GetKVPAsync<string>(DEVICE_ID);

                    if (String.IsNullOrEmpty(DeviceId))
                    {
                        DeviceId = UserDevices.First().Id;
                    }

                    return await LoadDevice();
                }
                else
                {
                    HasDevices = false;
                    NoDevices = !HasDevices;
                    return InvokeResult.Success;
                }
            });
        }


        public ObservableCollection<GeoFence> GeoFences { get; } = new ObservableCollection<GeoFence>();

        private bool _hasDevices;
        public bool HasDevices
        {
            get { return _hasDevices; }
            set { Set(ref _hasDevices, value); }
        }

        private bool _noDevices;
        public bool NoDevices
        {
            get { return _noDevices; }
            set { Set(ref _noDevices, value); }
        }

        public ObservableCollection<SensorSummary> Sensors { get; } = new ObservableCollection<SensorSummary>();

        public ObservableCollection<DeviceSummary> UserDevices
        {
            get => _userDevices;
            set => Set(ref _userDevices, value);
        }

        public bool IsNotFirstVessel
        {
            get => _isNotFirstVessel;
            set => Set(ref _isNotFirstVessel, value);
        }

        public bool IsNotLastVessel
        {
            get => _isNotLastVessel;
            set => Set(ref _isNotLastVessel, value);
        }

        public string SystemStatus
        {
            get => _systemStatus;
            set => Set(ref _systemStatus, value);
        }

        public GeoLocation CurrentVeseelLocation
        {
            get => _currentVeseelLocation;
            set => Set(ref _currentVeseelLocation, value);
        }

        private void _webSocket_MessageReceived(object sender, string json)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(json);
            HandleMessage(notification);
        }

        protected virtual void HandleMessage(Notification notification)
        {
            var warningSensors = new List<SensorSummary>();
            var outOfToleranceSensors = new List<SensorSummary>();

            switch (notification.PayloadType)
            {
                case nameof(Device):
                    var serializerSettings = new JsonSerializerSettings();
                    serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    var device = JsonConvert.DeserializeObject<Device>(notification.Payload, serializerSettings);
                    CurrentVeseelLocation = device.GeoLocation;


                    /*
                    foreach (var sensor in Sensors)
                    {
                        if (sensor.Technology.Value == SensorTechnology.ADC)
                        {
                            CurrentDevice.SensorCollection.AdcValues[sensor.Config.SensorIndex - 1] = device.Sensors.AdcValues[sensor.Config.SensorIndex - 1];
                            sensor.Value = device.Sensors.AdcValues[sensor.Config.SensorIndex - 1].ToString();
                        }

                        if (sensor.Technology == SensorTechnology.IO)
                        {
                            CurrentDevice.Sensors.IoValues[sensor.Config.SensorIndex - 1] = device.Sensors.IoValues[sensor.Config.SensorIndex - 1];
                            sensor.Value = device.Sensors.IoValues[sensor.Config.SensorIndex - 1].ToString();
                        }

                        /*if (sensor.Warning)
                        {
                            warningSensors.Add(sensor);
                        }

                        if (sensor.OutOfTolerance)
                        {
                            outOfToleranceSensors.Add(sensor);
                        }
                    }*/

                    break;
            }

            /*if (outOfToleranceSensors.Any())
            {
                HeaderBackgroundColor = Xamarin.Forms.Color.FromRgb(0xE9, 0x5C, 0x5D);
                HeaderForegroundColor = Xamarin.Forms.Color.White;
                SystemStatus = String.Join(", ", outOfToleranceSensors.Select(oot => oot.Config.Name + " " + oot.Value));
            }
            else if (warningSensors.Any())
            {
                HeaderBackgroundColor = Xamarin.Forms.Color.FromRgb(0xFF, 0xC8, 0x7F);
                HeaderForegroundColor = Xamarin.Forms.Color.White;
                SystemStatus = String.Join(", ", warningSensors.Select(oot => oot.Config.Name + " " + oot.Value));
            }
            else
            {
                HeaderBackgroundColor = Xamarin.Forms.Color.FromRgb(0x55, 0xA9, 0xF2);
                HeaderForegroundColor = Xamarin.Forms.Color.FromRgb(0x21, 0x21, 0x21);
                SystemStatus = "All systems nominal";
            }*/
        }

        public RelayCommand PreviousVesselCommand { get; }
        public RelayCommand NextVesselCommand { get; }

        private string GetChannelURI()
        {
            return $"/api/wsuri/device/{CurrentDevice.Id}/normal";
        }
    }
}
