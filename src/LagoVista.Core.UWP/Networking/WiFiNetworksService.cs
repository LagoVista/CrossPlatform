using LagoVista.Core.Networking.WiFi;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.UI.Core;

namespace LagoVista.Core.UWP.Networking
{
    public class WiFiNetworksService : IWiFiNetworksService
    {
        private Windows.Devices.WiFi.WiFiAdapter _adapter;

        private List<WiFiAvailableNetwork> _availableNetworks = new List<WiFiAvailableNetwork>();

        private WiFiAvailableNetwork _currentConnection;

        readonly ObservableCollection<WiFiConnection> _allConnections = new ObservableCollection<WiFiConnection>();
        readonly ObservableCollection<WiFiConnection> _filteredConnections = new ObservableCollection<WiFiConnection>();
        readonly ObservableCollection<string> _connectionFilters = new ObservableCollection<string>();

        public event EventHandler<WiFiNetworkSelectedEventArgs> SsidUpdated;
        public event EventHandler<WiFiConnection> Connected;
        public event EventHandler<WiFiConnection> Disconnected;

        private readonly CoreDispatcher _coreDispatcher;

        public ObservableCollection<WiFiConnection> AllConnections => _allConnections;

        public ObservableCollection<WiFiConnection> FilteredConnections => _filteredConnections;

        public ObservableCollection<string> SsidConnectionFilters => _connectionFilters;

        public string DefaultConnection => throw new NotImplementedException();

        public WiFiNetworksService(CoreDispatcher coreDispatcher)
        {
            _coreDispatcher = coreDispatcher;
        }

        public WiFiConnection CurrentConnection
        {
            get => (_currentConnection == null) ? AllConnections.Where(cn => cn.Ssid == _currentConnection.Ssid).FirstOrDefault() : null;
        }

        private WiFiConnection ToWiFiConnection(WiFiAvailableNetwork network)
        {
            return new WiFiConnection(network.Ssid, network.Ssid, network.Bssid,
                                  network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.None ||
                                   network.SecuritySettings.NetworkAuthenticationType == NetworkAuthenticationType.None)
            {
                SignalBars = network.SignalBars,
                SignalDB = network.NetworkRssiInDecibelMilliwatts
            };
        }

        private void RefreshNetworkList()
        {
            _availableNetworks.Clear();
            _filteredConnections.Clear();
            _allConnections.Clear();

            foreach (var network in _adapter.NetworkReport.AvailableNetworks)
            {
                if (!String.IsNullOrEmpty(network.Ssid))
                {
                    _availableNetworks.Add(network);
                }

                var wifiConnection = ToWiFiConnection(network);

                _allConnections.Add(wifiConnection);
                if (_filteredConnections.Any())
                {
                    if (_allConnections.Any(cn => _connectionFilters.Any(flt => cn.Ssid.ToLower().Contains(flt.ToLower()))))
                    {
                        _filteredConnections.Add(wifiConnection);
                    }
                }
                else
                {
                    _filteredConnections.Add(wifiConnection);
                }
            }
        }

        public async Task StartAsync(Core.Networking.WiFi.WiFiAdapter adapter)
        {
            if (_adapter != null)
            {
                _adapter.AvailableNetworksChanged -= _adapter_AvailableNetworksChanged;
            }

            _adapter = await Windows.Devices.WiFi.WiFiAdapter.FromIdAsync(adapter.Id);
            
            _adapter.AvailableNetworksChanged += _adapter_AvailableNetworksChanged;

            await _adapter.ScanAsync();
        }

        private async void _adapter_AvailableNetworksChanged(Windows.Devices.WiFi.WiFiAdapter sender, object args)
        {
            await _coreDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RefreshNetworkList();
            });
        }

        public async Task<InvokeResult> ConnectAsync(WiFiConnection connection)
        {
            if (_adapter == null)
            {
                throw new Exception("No adapter set.");
            }

            _currentConnection = _adapter.NetworkReport.AvailableNetworks.Where(net => net.Ssid == connection.Ssid).FirstOrDefault();

            if (_currentConnection == null)
            {
                return InvokeResult.FromError($"Could not find network ${connection.Ssid}");
            }

            var result = await _adapter.ConnectAsync(_currentConnection, WiFiReconnectionKind.Automatic);
            connection.IsConnected = result.ConnectionStatus == WiFiConnectionStatus.Success;

            return (result.ConnectionStatus == WiFiConnectionStatus.Success) ? InvokeResult.Success : InvokeResult.FromError($"Could not connect to ${connection.Ssid} - {result.ConnectionStatus}");
        }

        public Task<InvokeResult> DisconnectAsync(WiFiConnection connection)
        {
            if (_adapter == null)
            {
                throw new Exception("No adapter set.");
            }

            var network = _adapter.NetworkReport.AvailableNetworks.Where(net => net.Ssid == connection.Ssid).FirstOrDefault();

            _adapter.Disconnect();

            _currentConnection = null;

            return Task.FromResult(InvokeResult.Success);
        }

        public Task StopAsync()
        {
            _adapter.AvailableNetworksChanged -= _adapter_AvailableNetworksChanged;
            _adapter = null;

            _allConnections.Clear();
            _availableNetworks.Clear();
            _filteredConnections.Clear();

            return Task.CompletedTask;

        }
    }
}
