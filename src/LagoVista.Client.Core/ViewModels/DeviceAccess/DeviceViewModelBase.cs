using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Net;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.Geo;
using LagoVista.Core.Validation;
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
        GeoLocation _currentVeseelLocation;
      
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
                    _webSocket.MessageReceived += WebSocket_MessageReceived;
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
                _gattConnection.DeviceDiscovered += GattConnection_DeviceDiscovered;
                _gattConnection.DeviceDisconnected += BtSerial_DeviceDisconnected;
                _gattConnection.CharacteristicChanged += GattConnection_CharacteristicRead;
               
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

        private async void GattConnection_DeviceDiscovered(object sender, BLEDevice e)
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
            if (_gattConnection != null)
            {

                _gattConnection.DeviceConnected -= BtSerial_DeviceConnected;
                _gattConnection.DeviceDisconnected -= BtSerial_DeviceDisconnected;
                _gattConnection.DeviceDiscovered -= GattConnection_DeviceDiscovered;
                _gattConnection.CharacteristicChanged -= GattConnection_CharacteristicRead;

                await _gattConnection.StopScanAsync();
            }

            if (_webSocket != null)
            {
                await _webSocket.CloseAsync();
                Debug.WriteLine("Web Socket is Closed.");
                _webSocket = null;
            }
        }

        private void GattConnection_CharacteristicRead(object sender, BLECharacteristicsValue e)
        {
            Debug.WriteLine(e.Uid + " " + e.Value);
            BLECharacteristicRead(e);
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

        protected virtual void BLECharacteristicRead(BLECharacteristicsValue characteristic){ }
        protected virtual void OnBLEDevice_Connected(BLEDevice device) { }
        protected virtual void OnBLEDevice_Disconnected(BLEDevice device) { }

 
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

                if(GattConnection != null)
                await GattConnection.StopScanAsync();

                CurrentDevice = null;
                var deviceResponse = await DeviceManagementClient.GetDeviceAsync(AppConfig.DeviceRepoId, DeviceId);
                if (deviceResponse.Successful)
                {
                    CurrentDevice = deviceResponse.Model;
                    await DeviceLoadedAsync(CurrentDevice);

                    if (CurrentDevice.GeoLocation != null)
                    {
                        CurrentDeviceLocation = CurrentDevice.GeoLocation;
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

                if (GattConnection != null)
                {
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
                }

                return deviceResponse.ToInvokeResult();
            });
        }

        protected virtual Task DeviceLoadedAsync(Device device)
        {
            return Task.CompletedTask;
        }

        public ObservableCollection<GeoFence> GeoFences { get; } = new ObservableCollection<GeoFence>();

        public ObservableCollection<SensorSummary> Sensors { get; } = new ObservableCollection<SensorSummary>();

        public GeoLocation CurrentDeviceLocation
        {
            get => _currentVeseelLocation;
            set => Set(ref _currentVeseelLocation, value);
        }

        private void WebSocket_MessageReceived(object sender, string json)
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
                    CurrentDeviceLocation = device.GeoLocation;

                    var sensor1 = device.SensorCollection.First();
                
                    foreach (var sensor in Sensors)
                    {
                        if (sensor.Config.Technology.Value == SensorTechnology.ADC)
                        {
//                            CurrentDevice.SensorCollection.AdcValues[sensor.Config.SensorIndex - 1] = device.Sensors.AdcValues[sensor.Config.SensorIndex - 1];
      //                      sensor.Value = device.Sensors.AdcValues[sensor.Config.SensorIndex - 1].ToString();
                        }

                        if (sensor.Config.Technology.Value == SensorTechnology.IO)
                        {
  //                          CurrentDevice.Sensors.IoValues[sensor.Config.SensorIndex - 1] = device.Sensors.IoValues[sensor.Config.SensorIndex - 1];
    //                        sensor.Value = device.Sensors.IoValues[sensor.Config.SensorIndex - 1].ToString();
                        }

                        /*if (sensor.Warning)
                        {
                            warningSensors.Add(sensor);
                        }

                        if (sensor.OutOfTolerance)
                        {
                            outOfToleranceSensors.Add(sensor);
                        }*/
                    }

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

        
        private string GetChannelURI()
        {
            return $"/api/wsuri/device/{CurrentDevice.Id}/normal";
        }
    }
}
