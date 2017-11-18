using LagoVista.Core.Commanding;
using LagoVista.Core.UWP.Networking;
using LagoVista.Core.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;

namespace LagoVista.Core.UWP.ViewModels.Common
{
    public class NetworkSettingsViewModel : ViewModelBase
    {
        RelayCommand _refreshWiFiCommand;
        RelayCommand _connectToWiFiCommand;
        RelayCommand _cancelConnectToWiFiCommand;
        RelayCommand _nextCommand;

        public class SettingsSection
        {
            public String Name { get; set; }
            public String Key { get; set; }
            public bool IsVisible { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        private const string WIFI_Initial_State = "WiFiInitialState";
        private const string WIFI_Connecting_State = "WiFiConnectingState";
        private const string WIFI_Password_State = "WiFiPasswordState";

        public NetworkSettingsViewModel()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            _refreshWiFiCommand = new RelayCommand(RefreshWiFi);
            _connectToWiFiCommand = new RelayCommand(ConnectToWiFi);
            _cancelConnectToWiFiCommand = new RelayCommand(CancelConnectToWiFi);
            _nextCommand = new RelayCommand(Next);

            SettingsSections.Add(new SettingsSection() { Name = "Wired Networks", Key = "WIRED" });
            SettingsSections.Add(new SettingsSection() { Name = "WiFi Networks", Key = "WIFI" });

            SelectedSection = SettingsSections.First();
        }

        List<SettingsSection> _sections = new List<SettingsSection>();
        public List<SettingsSection> SettingsSections { get { return _sections; } }


        SettingsSection _selectedSection;
        public SettingsSection SelectedSection
        {
            get { return _selectedSection; }
            set
            {
                if (_selectedSection != value)
                {
                    Set(ref _selectedSection, value);
                    foreach (var setting in _sections)
                        RaisePropertyChanged(setting.Key);
                }
            }
        }

        private async void RefreshNetworkDisplay()
        {
            SetupEthernet();
            await SetupWifi();
        }
       

        public override Task InitAsync()
        {
            RefreshNetworkDisplay();
            return base.InitAsync();
        }

        public override Task ReloadedAsync()
        {
            RefreshNetworkDisplay();
            return base.ReloadedAsync();
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            Core.PlatformSupport.Services.DispatcherServices.Invoke(() =>
            {
                RefreshNetworkDisplay();
            });
        }

        private void ShowCurrent()
        {
            CurrentWiFiConnection = WiFiNetworks.Instance.GetCurrentWifiNetwork();
        }

        private void SetupEthernet()
        {
            DirectConnection = WiredNetworks.Instance.GetDirectConnection();
        }

        private async Task SetupWifi()
        {
            if (await WiFiNetworks.Instance.IsWiFiAvailable())
            {
                AvailableWiFiNetworks = await WiFiNetworks.Instance.GetAvailableNetworks();
                ShowCurrent();
            }
            else
                AvailableWiFiNetworks = null;
        }


        private void ConnectToWiFi()
        {
            if (WiFiNetworks.IsNetworkOpen(SelectedNetwork.WiFiAvailableNetwork))
                ConnectToWifi(SelectedNetwork);
            else
                SelectedNetwork.State = WIFI_Password_State;
        }

        private async void ConnectToWifi(WiFiNetwork network, PasswordCredential credential = null)
        {
            var didConnect = credential == null ?
                WiFiNetworks.Instance.ConnectToNetwork(network.WiFiAvailableNetwork, network.ConnectAutomatically) :
                WiFiNetworks.Instance.ConnectToNetworkWithPassword(network.WiFiAvailableNetwork, network.ConnectAutomatically, credential);

            Core.PlatformSupport.Services.DispatcherServices.Invoke(() =>
            {
                SelectedNetwork.State = WIFI_Connecting_State;
            });

            var connected = await didConnect;

            Core.PlatformSupport.Services.DispatcherServices.Invoke(async () =>
            {
                SelectedNetwork.State = WIFI_Initial_State;
                await SetupWifi();
            });
        }

        private void TryConnect()
        {
            var credential = (string.IsNullOrEmpty(WiFiPassword)) ? (PasswordCredential)null : new PasswordCredential() { Password = WiFiPassword };
            ConnectToWifi(SelectedNetwork, credential);
        }

        private void Next()
        {
            TryConnect();
        }

        private void CancelConnectToWiFi()
        {
            SelectedNetwork.State = WIFI_Initial_State;
            SelectedNetwork = null;
        }

        private async void RefreshWiFi()
        {
            await SetupWifi();
        }
       
        private WiFiAvailableNetwork _currentWiFiConnection;
        public WiFiAvailableNetwork CurrentWiFiConnection
        {
            get { return _currentWiFiConnection; }
            set { Set(ref _currentWiFiConnection, value); }
        }

        private ConnectionProfile _directConnection;
        public ConnectionProfile DirectConnection
        {
            get { return _directConnection; }
            set { Set(ref _directConnection, value); }
        }

        List<WiFiNetwork> _availableWiFiNetworks;
        public List<WiFiNetwork> AvailableWiFiNetworks
        {
            get { return _availableWiFiNetworks; }
            set { Set(ref _availableWiFiNetworks, value); }
        }

        private String _wifiPassword;
        public String WiFiPassword
        {
            get { return _wifiPassword; }
            set { Set(ref _wifiPassword, value); }
        }

        WiFiNetwork _selectedNetwork = null;
        public WiFiNetwork SelectedNetwork
        {
            get { return _selectedNetwork; }
            set
            {
                if (_selectedNetwork != null)
                    _selectedNetwork.State = WIFI_Initial_State;

                Set(ref _selectedNetwork, value);

                if (_selectedNetwork != null)
                    _selectedNetwork.State = WIFI_Password_State;
            }
        }

        public RelayCommand RefreshWiFiCommand { get { return _refreshWiFiCommand; } }
        public RelayCommand ConnectToWiFiCommand { get { return _connectToWiFiCommand; } }
        public RelayCommand NextCommand { get { return _nextCommand; } }
        public RelayCommand CancelConnectToWiFiCommand { get { return _cancelConnectToWiFiCommand; } }
    }
}