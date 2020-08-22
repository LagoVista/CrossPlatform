using LagoVista.Client.Core.Interfaces; 
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Sample
{
    public class BTSerialViewModel : XPlatViewModel
    {
        IBluetoothSerial _btSerial;

        public BTSerialViewModel()
        {
            _btSerial = SLWIOC.Create<IBluetoothSerial>();
        }

        public override async Task InitAsync()
        {
            Devices = await _btSerial.SearchAsync();

            await base.InitAsync();
        }

        ObservableCollection<BTDevice> _devices;
        public ObservableCollection<BTDevice> Devices
        {
            get { return _devices; }
            private set { Set(ref _devices, value); }
        }

        private async void Connect(BTDevice device)
        {
            await _btSerial.ConnectAsync(device);
        }

        private BTDevice _selectedDevice;
        public BTDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                Set(ref _selectedDevice, value);
                if(_selectedDevice != null)
                {
                    Connect(value);
                }
            }
        }
    }
}
