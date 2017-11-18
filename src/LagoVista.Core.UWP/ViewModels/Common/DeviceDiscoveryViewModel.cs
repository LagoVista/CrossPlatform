using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using LagoVista.Core.Networking.Models;

namespace LagoVista.Core.UWP.ViewModels.Common
{
    public class DeviceDiscoveryViewModel : ViewModelBase
    {
        private Services.SSDPClient _finder;
        private ObservableCollection<uPnPDevice> _devices = new ObservableCollection<uPnPDevice>();
        private List<String> _rootDevicesSoFar = new List<string>();


        public async override Task InitAsync()
        {
            _finder = new Services.SSDPClient();
            _finder.NewDeviceFound += _finder_NewDeviceFound;

            await Task.Delay(1);

            string item;

            item = "urn:schemas-upnp-org:device:ZonePlayer:1";  // Sonos hardware
            item = "urn:schemas-upnp-org:device:Basic:1";       // eg my Home Server
            item = "urn:schemas-upnp-org:device:MediaServer:1"; // eg my PCs
            item = "ssdp:all";                                   // everything (NOT * as it was previously)

            await _finder.SsdpQueryAsync(item, 10);

            //_finder.FindAsync(item, 4, foundResult, completedSearching);
        }

        private void _finder_NewDeviceFound(object sender, uPnPDevice e)
        {
            _devices.Add(e);
            Debug.WriteLine(e.ToString());
        }


        public ObservableCollection<uPnPDevice> Devices { get { return _devices; } }
    }
}
