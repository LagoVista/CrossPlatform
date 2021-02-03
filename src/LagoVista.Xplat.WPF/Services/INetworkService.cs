using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.XPlat.WPF.Services
{

    public class NetworkService : INetworkService
    {
        public bool IsInternetConnected => true;

        public ObservableCollection<NetworkDetails> AllConnections => throw new NotImplementedException();

        public event EventHandler NetworkInformationChanged;

        public string GetIPV4Address()
        {
            throw new NotImplementedException();
        }

        public void OpenURI(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAysnc()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TestConnectivityAsync()
        {
            throw new NotImplementedException();
        }
    }
}
