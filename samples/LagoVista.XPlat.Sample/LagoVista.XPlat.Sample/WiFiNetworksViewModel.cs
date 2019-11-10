using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Networking.WiFi;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Sample
{
    public class WiFiNetworksViewModel : XPlatViewModel
    {
        IWiFiAdapters _adaptersService;
        IWiFiNetworks _networksServices;

        public WiFiNetworksViewModel(IWiFiAdapters adapters, IWiFiNetworks networks)
        {
            _adaptersService = adapters;
            _networksServices = networks;
        }

        public async override Task InitAsync()
        {
            this.Adapters = await _adaptersService.GetAdapterListAsync();

            var defaultAdapter = this.Adapters.First();
            if (defaultAdapter != null)
            {
                await _networksServices.StartAsync(defaultAdapter);
            }
        }

        ObservableCollection<WiFiAdapter> _adapters;

        public ObservableCollection<WiFiAdapter> Adapters
        {
            get => _adapters;
            set => Set(ref _adapters, value);
        }

        public ObservableCollection<WiFiConnection> Networks => _networksServices.AllConnections;

        private async void Connect(WiFiConnection connection)
        {
            var reuslt = await _networksServices.ConnectAsync(connection);
        }

        private async void Disconnect(WiFiConnection connection)
        {
            await _networksServices.DisconnectAsync(_selectedConnection);
        }

        WiFiConnection _selectedConnection;
        public WiFiConnection SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {                
                if (value != null)
                {
                    Connect(value);
                }
                else
                {
                    if (_selectedConnection != null)
                    {
                        Disconnect(_selectedConnection);
                    }
                }

                Set(ref _selectedConnection, value);
            }
        }
    }
}
