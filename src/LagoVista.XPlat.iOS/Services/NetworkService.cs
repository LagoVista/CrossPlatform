using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Net;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models;
using UIKit;
using Foundation;

namespace LagoVista.XPlat.iOS.Services
{
    public class NetworkService : INetworkService
    {
        private const string HOST_TO_TEST = "www.microsoft.com";

        public ObservableCollection<NetworkDetails> AllConnections
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsInternetConnected
        {
            get
            {
                return Reachability.Reachability.IsHostReachable(HOST_TO_TEST);
            }
        }

        public event EventHandler NetworkInformationChanged;

        public void RaiseNetworkInfoChanged()
        {
            NetworkInformationChanged?.Invoke(this, null);
        }


        public string GetIPV4Address()
        {
            var addresses = Dns.GetHostAddresses(Dns.GetHostName());
            string ipAddress = string.Empty;
            if (addresses != null && addresses[0] != null)
            {
                return addresses[0].ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        public Task RefreshAysnc()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestConnectivityAsync()
        {
            throw new NotImplementedException();
        }

        public void OpenURI(Uri uri)
        {            
            UIApplication.SharedApplication.OpenUrl(new NSUrl(uri.ToString()));
        }
    }
}