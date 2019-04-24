using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Sample
{
    public class NetworkingViewModel : XPlatViewModel
    {
        public async override Task InitAsync()
        {
            var srvc = SLWIOC.Get<INetworkService>();
            await srvc.RefreshAysnc();
            Networks = srvc.AllConnections;

            await base.InitAsync();
        }


        ObservableCollection<NetworkDetails> _networks;
        public ObservableCollection<NetworkDetails> Networks
        {
            get { return _networks; }
            set { Set(ref _networks, value); }
        }

    }
}
