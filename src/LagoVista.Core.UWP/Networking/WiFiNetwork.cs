using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFi;

namespace LagoVista.Core.UWP.Networking
{
    public class WiFiNetwork : ModelBase
    {
        WiFiAvailableNetwork _wifiAvailableNetwork;

        public WiFiNetwork(WiFiAvailableNetwork wifiAvailableNetwork)
        {
            _wifiAvailableNetwork = wifiAvailableNetwork;
        }

        public WiFiAvailableNetwork WiFiAvailableNetwork { get { return _wifiAvailableNetwork; } }

        private bool _connectAutomatically = false;
        public bool ConnectAutomatically
        {
            get { return _connectAutomatically; }
            set
            {
                _connectAutomatically = value;
                RaisePropertyChanged();
            }
        }

        private string _password;
        public String Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }

        private String _state;
        public String State
        {
            get { return _state; }
            set { Set(ref _state, value); }
        }
    }
}
