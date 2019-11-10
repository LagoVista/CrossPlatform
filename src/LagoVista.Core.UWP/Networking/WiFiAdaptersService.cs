using LagoVista.Core.Networking.WiFi;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;

namespace LagoVista.Core.UWP.Networking
{
    public class WiFiAdaptersService : IWiFiAdaptersService
    {
        DeviceWatcher _wifiAdapterWatcher;

        Dictionary<String, LagoVista.Core.Networking.WiFi.WiFiAdapter> _wifiAdapters = new Dictionary<string, LagoVista.Core.Networking.WiFi.WiFiAdapter>();
        ManualResetEvent EnumAdaptersCompleted = new ManualResetEvent(false);
        private static WiFiAccessStatus? accessStatus;

        public WiFiAdaptersService()
        {
            _wifiAdapterWatcher = DeviceInformation.CreateWatcher(Windows.Devices.WiFi.WiFiAdapter.GetDeviceSelector());
            _wifiAdapterWatcher.EnumerationCompleted += AdaptersEnumCompleted;
            _wifiAdapterWatcher.Added += AdaptersAdded;
            _wifiAdapterWatcher.Removed += AdaptersRemoved;
            _wifiAdapterWatcher.Start();
        }

        private async void AdaptersEnumCompleted(DeviceWatcher sender, object args)
        {
            var adapters = await DeviceInformation.FindAllAsync(Windows.Devices.WiFi.WiFiAdapter.GetDeviceSelector());

            _wifiAdapters.Clear();

            foreach (var adapter in adapters)
            {
                try
                {                   
                    _wifiAdapters[adapter.Id] = new LagoVista.Core.Networking.WiFi.WiFiAdapter(adapter.Name, adapter.Id);
                }
                catch (Exception)
                {
                    _wifiAdapters.Remove(adapter.Id);
                }
            }

            EnumAdaptersCompleted.Set();
        }

        private Task UpdateAdapters()
        {
            bool fInit = false;
            foreach (var adapter in _wifiAdapters)
            {
                if (adapter.Value == null)
                {
                    fInit = true;
                }
            }

            if (fInit)
            {
                List<String> WiFiAdaptersID = new List<string>(_wifiAdapters.Keys);
                for (int i = 0; i < WiFiAdaptersID.Count; i++)
                {
                    string id = WiFiAdaptersID[i];
                    try
                    {
                        _wifiAdapters[id] = new LagoVista.Core.Networking.WiFi.WiFiAdapter(_wifiAdapters[id].Name, id);
                    }
                    catch (Exception)
                    {
                        _wifiAdapters.Remove(id);
                    }
                }
            }

            return Task.CompletedTask;
        }


        private void AdaptersRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _wifiAdapters.Remove(args.Id);
        }

        private void AdaptersAdded(DeviceWatcher sender, DeviceInformation args)
        {
            _wifiAdapters.Add(args.Id, null);
        }

        public async Task<InvokeResult> CheckAuthroizationAsync()
        {
            if (!accessStatus.HasValue)
            {
                accessStatus = await Windows.Devices.WiFi.WiFiAdapter.RequestAccessAsync();
            }

            return (accessStatus == WiFiAccessStatus.Allowed) ? InvokeResult.Success : InvokeResult.FromError("WiFi access is not authorized for this application.");
        }

        public Task<ObservableCollection<LagoVista.Core.Networking.WiFi.WiFiAdapter>> GetAdapterListAsync()
        {
            EnumAdaptersCompleted.WaitOne();

            return Task.FromResult(new ObservableCollection<LagoVista.Core.Networking.WiFi.WiFiAdapter>(_wifiAdapters.Values));
        }


    }
}
