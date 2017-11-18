using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Security.Credentials;

namespace LagoVista.Core.UWP.Networking
{
    public class WiFiNetworks : NetworkBase
    {
        private Dictionary<WiFiAvailableNetwork, WiFiAdapter> networkNameToInfo;

        private static WiFiAccessStatus? accessStatus;

//        readonly static uint WirelessInterfaceIanaType = 71;
        Dictionary<String, WiFiAdapter> _wifiAdapters = new Dictionary<string, WiFiAdapter>();
        DeviceWatcher _wifiAdapterWatcher;
        ManualResetEvent EnumAdaptersCompleted = new ManualResetEvent(false);

        static WiFiNetworks _instance = new WiFiNetworks();

        private WiFiNetworks()
        {
            _wifiAdapterWatcher = DeviceInformation.CreateWatcher(WiFiAdapter.GetDeviceSelector());
            _wifiAdapterWatcher.EnumerationCompleted += AdaptersEnumCompleted;
            _wifiAdapterWatcher.Added += AdaptersAdded;
            _wifiAdapterWatcher.Removed += AdaptersRemoved;
            _wifiAdapterWatcher.Start();
        }

        private void AdaptersRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _wifiAdapters.Remove(args.Id);
        }

        private void AdaptersAdded(DeviceWatcher sender, DeviceInformation args)
        {
            _wifiAdapters.Add(args.Id, null);
        }

        private async void AdaptersEnumCompleted(DeviceWatcher sender, object args)
        {
            List<String> WiFiAdaptersID = new List<string>(_wifiAdapters.Keys);
            for (int i = 0; i < WiFiAdaptersID.Count; i++)
            {
                string id = WiFiAdaptersID[i];
                try
                {
                    _wifiAdapters[id] = await WiFiAdapter.FromIdAsync(id);
                }
                catch (Exception)
                {
                    _wifiAdapters.Remove(id);
                }
            }

            EnumAdaptersCompleted.Set();
        }

        public static WiFiNetworks Instance { get { return _instance; } }

        private async Task UpdateAdapters()
        {
            bool fInit = false;
            foreach (var adapter in _wifiAdapters)
            {
                if (adapter.Value == null)
                    fInit = true;
            }

            if (fInit)
            {
                List<String> WiFiAdaptersID = new List<string>(_wifiAdapters.Keys);
                for (int i = 0; i < WiFiAdaptersID.Count; i++)
                {
                    string id = WiFiAdaptersID[i];
                    try
                    {
                        _wifiAdapters[id] = await WiFiAdapter.FromIdAsync(id);
                    }
                    catch (Exception)
                    {
                        _wifiAdapters.Remove(id);
                    }
                }
            }
        }

        public async Task<bool> IsWiFiAvailable()
        {
            if ((await TestAccess()) == false)
                return false;

            try
            {
                EnumAdaptersCompleted.WaitOne();
                await UpdateAdapters();
                return (_wifiAdapters.Count > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public WiFiAvailableNetwork GetCurrentWifiNetwork()
        {
            var connectionProfiles = NetworkInformation.GetConnectionProfiles();

            if (connectionProfiles.Count < 1)
                return null;

            var validProfiles = connectionProfiles.Where(profile =>
            {
                return (profile.IsWlanConnectionProfile && profile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.None);
            });

            if (validProfiles.Count() < 1)
                return null;

            var firstProfile = validProfiles.First() as ConnectionProfile;

            return networkNameToInfo.Keys.FirstOrDefault(wifiNetwork => wifiNetwork.Ssid.Equals(firstProfile.ProfileName));
        }


        private static async Task<bool> TestAccess()
        {
            if (!accessStatus.HasValue)
                accessStatus = await WiFiAdapter.RequestAccessAsync();

            return (accessStatus == WiFiAccessStatus.Allowed);
        }

        private async Task<bool> UpdateInfo()
        {
            if ((await TestAccess()) == false)
                return false;

            networkNameToInfo = new Dictionary<WiFiAvailableNetwork, WiFiAdapter>();
            List<WiFiAdapter> WiFiAdaptersList = new List<WiFiAdapter>(_wifiAdapters.Values);
            foreach (var adapter in WiFiAdaptersList)
            {
                if (adapter == null)
                    return false;

                await adapter.ScanAsync();

                if (adapter.NetworkReport == null)
                    continue;

                foreach (var network in adapter.NetworkReport.AvailableNetworks)
                {
                    if (!HasSsid(networkNameToInfo, network.Ssid))
                        networkNameToInfo[network] = adapter;
                }
            }

            return true;
        }

        private bool HasSsid(Dictionary<WiFiAvailableNetwork, WiFiAdapter> resultCollection, string ssid)
        {
            foreach (var network in resultCollection)
            {
                if (!string.IsNullOrEmpty(network.Key.Ssid) && network.Key.Ssid == ssid)
                    return true;
            }

            return false;
        }

        public async Task<List<WiFiNetwork>> GetAvailableNetworks()
        {
            await UpdateInfo();

            var networks = new List<WiFiNetwork>();
            foreach (var network in networkNameToInfo.Keys)
                networks.Add(new WiFiNetwork(network));

            return networks;
        }

        public async Task<bool> ConnectToNetwork(WiFiAvailableNetwork network, bool autoConnect)
        {
            if (network == null)
            {
                return false;
            }

            var result = await networkNameToInfo[network].ConnectAsync(network, autoConnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual);

            return (result.ConnectionStatus == WiFiConnectionStatus.Success);
        }

        public void DisconnectNetwork(WiFiAvailableNetwork network)
        {
            networkNameToInfo[network].Disconnect();
        }

        public static bool IsNetworkOpen(WiFiAvailableNetwork network)
        {
            return network.SecuritySettings.NetworkEncryptionType == NetworkEncryptionType.None;
        }


        public async Task<bool> ConnectToNetworkWithPassword(WiFiAvailableNetwork network, bool autoConnect, PasswordCredential password)
        {
            if (network == null)
                return false;

            var result = await networkNameToInfo[network].ConnectAsync(
                network,
                autoConnect ? WiFiReconnectionKind.Automatic : WiFiReconnectionKind.Manual,
                password);

            return (result.ConnectionStatus == WiFiConnectionStatus.Success);
        }


     
    }
}
