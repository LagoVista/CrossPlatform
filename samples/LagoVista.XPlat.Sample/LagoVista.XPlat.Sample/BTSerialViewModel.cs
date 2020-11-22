using LagoVista.Client.Core.Interfaces; 
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
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
            _btSerial.DFUProgress += _btSerial_DFUProgress;
            _btSerial.DFUCompleted += _btSerial_DFUCompleted;
            _btSerial.DFUFailed += _btSerial_DFUFailed;

            _btSerial.DeviceConnected += _btSerial_DeviceConnected;
            _btSerial.DeviceConnecting += _btSerial_DeviceConnecting;
            _btSerial.DeviceDisconnected += _btSerial_DeviceDisconnected;
            _btSerial.ReceivedLine += _btSerial_ReceivedLine;

            SendDFUCommand = new RelayCommand(SendDFU);
        }

        
        private void _btSerial_ReceivedLine(object sender, string e)
        {
            var lines = e.Split('\r');
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                   
                }
            }
        }


        private void _btSerial_DeviceDisconnected(object sender, BTDevice e)
        {
            Log.Insert(0, "Device Disconnected - " + e.DeviceName);
        }

        private void _btSerial_DeviceConnecting(object sender, BTDevice e)
        {
            Log.Insert(0, "Device Connecting - " + e.DeviceName);
        }

        private void _btSerial_DeviceConnected(object sender, BTDevice e)
        {
            Log.Insert(0, "Device Connected" + e.DeviceName);
        }

        private void _btSerial_DFUFailed(object sender, string e)
        {
            Log.Insert(0, "DFU Failed.");
        }

        private void _btSerial_DFUCompleted(object sender, EventArgs e)
        {
            Log.Insert(0, "DFU Completed.");
        }

        private void _btSerial_DFUProgress(object sender, DFUProgress e)
        {
            Log.Insert(0, $"Progress {e.Progress}%");
        }

        public override async Task IsClosingAsync()
        {
            if(SelectedDevice != null)
            {
                IsBusy = true;
                await _btSerial.SendAsync($"QUIT\n");
                await Task.Delay(1000);
                await _btSerial.DisconnectAsync(SelectedDevice);
                IsBusy = false;
            }

            await base.IsClosingAsync();
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
            await _btSerial.SendAsync($"HELLO\n");
            await _btSerial.SendAsync($"QUERY\n");
        }

        public async void SendDFU()
        {
            var rnd = new Random();

            var dfu = new byte[1024 * 1024 + 20 * 30];
            for(var idx = 0; idx < dfu.Length; ++idx)
            {
                dfu[idx] = (byte)(rnd.Next() & 0xFF);
            }

            await _btSerial.SendDFUAsync(SelectedDevice, dfu);
        }

        private BTDevice _selectedDevice;
        public BTDevice SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (_selectedDevice != value)
                {
                    Set(ref _selectedDevice, value);
                    if (_selectedDevice != null)
                    {
                        Connect(value);
                    }
                }
            }
        }
        public ObservableCollection<string> Data { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Log{ get; set; } = new ObservableCollection<string>();

        public RelayCommand SendDFUCommand { get; private set; }
    }
}
