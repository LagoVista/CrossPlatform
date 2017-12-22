using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.Content;
using System.Net;
using Android.Net;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Models;

namespace LagoVista.XPlat.Droid.Services
{
    public class NetworkService : INetworkService
    {
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
                var context = global::Android.App.Application.Context;

                var connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                if (connectivityManager == null)
                {
                    return false;
                }

                return connectivityManager.ActiveNetworkInfo == null ? false : connectivityManager.ActiveNetworkInfo.IsConnected;
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

        public void OpenURI(System.Uri uri)
        {
            Xamarin.Forms.Device.OpenUri(uri);
        }
    }
}