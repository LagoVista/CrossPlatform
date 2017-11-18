using LagoVista.Common.Net.Models;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace LagoVista.Core.UWP.Networking
{
    public class NetworkBase
    {
        readonly static uint WirelessInterfaceIanaType = 71;

        public static string GetCurrentNetworkName()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null)
                return icp.ProfileName;
     
            return "No Internet Connection";
        }

        public static string GetCurrentIpv4Address()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null && icp.NetworkAdapter != null && icp.NetworkAdapter.NetworkAdapterId != null)
            {
                var name = icp.ProfileName;

                var hostnames = NetworkInformation.GetHostNames();

                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null &&
                        hn.IPInformation.NetworkAdapter != null &&
                        hn.IPInformation.NetworkAdapter.NetworkAdapterId != null &&
                        hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId &&
                        hn.Type == HostNameType.Ipv4)
                    {
                        return hn.CanonicalName;
                    }
                }
            }


            return "-";
        }

        public static async Task<IList<NetworkInfo>> GetNetworkInformation()
        {
            var networkList = new Dictionary<Guid, NetworkInfo>();
            var hostNamesList = NetworkInformation.GetHostNames();
            var resourceLoader = ResourceLoader.GetForCurrentView();

            foreach (var hostName in hostNamesList)
            {
                if ((hostName.Type == HostNameType.Ipv4 || hostName.Type == HostNameType.Ipv6) &&
                    (hostName != null && hostName.IPInformation != null && hostName.IPInformation.NetworkAdapter != null))
                {
                    var profile = await hostName.IPInformation.NetworkAdapter.GetConnectedProfileAsync();
                    if (profile != null)
                    {
                        NetworkInfo info;

                        if (!networkList.TryGetValue(hostName.IPInformation.NetworkAdapter.NetworkAdapterId, out info))
                        {
                            info = new NetworkInfo();
                            networkList[hostName.IPInformation.NetworkAdapter.NetworkAdapterId] = info;
                            if (hostName.IPInformation.NetworkAdapter.IanaInterfaceType == WirelessInterfaceIanaType && profile.ProfileName.Equals("Ethernet"))
                                info.NetworkName = "Wireless LAN Adapter";
                            else
                                info.NetworkName = profile.ProfileName;

                            var statusTag = profile.GetNetworkConnectivityLevel().ToString();
                            info.NetworkStatus = "Network Type:" + statusTag;
                        }

                        if (hostName.Type == HostNameType.Ipv4)
                            info.NetworkIpv4 = hostName.CanonicalName;
                        else
                            info.NetworkIpv6 = hostName.CanonicalName;
                    }
                }
            }

            var res = new List<NetworkInfo>();
            res.AddRange(networkList.Values);
            return res;
        }
    }
}
