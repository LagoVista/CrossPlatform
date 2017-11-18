using Windows.Networking.Connectivity;

namespace LagoVista.Core.UWP.Networking
{
    public class WiredNetworks : NetworkBase
    {
        private readonly static uint EthernetIanaType = 6;
        static WiredNetworks _instance = new WiredNetworks();

        public ConnectionProfile GetDirectConnection()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp != null)
            {
                if (icp.NetworkAdapter.IanaInterfaceType == EthernetIanaType)
                {
                    return icp;
                }
            }

            return null;
        }

        public static WiredNetworks Instance { get { return _instance; } }
    }
}
