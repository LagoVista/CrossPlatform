using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.XPlat.Sample.ViewModels
{
    public class GATTConnectionViewModel : XPlatViewModel
    {
        IGATTConnection _gattConnection;
        public GATTConnectionViewModel(IGATTConnection gattConnection)
        {
            _gattConnection = gattConnection ?? throw new ArgumentNullException(nameof(gattConnection));
            _gattConnection.RegisterKnownServices(NuvIoTGATTProfile.GetNuvIoTGATT().Services);
            _gattConnection.DeviceDisconnected += _connectionStateChanged;
            _gattConnection.DeviceConnected += _connectionStateChanged;
            ScanCommand = new RelayCommand(ScanAsync);
            ConnectCommand = new RelayCommand(ConnectAsync, () => SelectedDevice != null && !SelectedDevice.Connected);
            DisconnectCommand = new RelayCommand(DisconnectAsync, () => SelectedDevice != null && SelectedDevice.Connected);

            SubscribeCommand = new RelayCommand(SubscribeCharacteristic, () => SelectedCharacteristic != null && SelectedCharacteristic.Properties.Contains(BLECharacteristicPropertyTypes.Notify));
            UnsubscribeCommand = new RelayCommand(UnsubscribeCharacteristic, () => SelectedCharacteristic != null && SelectedCharacteristic.Properties.Contains(BLECharacteristicPropertyTypes.Notify));
            ReadCommand = new RelayCommand(ReadCharacteristic, () => SelectedCharacteristic != null && SelectedCharacteristic.Properties.Contains(BLECharacteristicPropertyTypes.Read));
            WriteCommand = new RelayCommand(WriteCharacteristic, () => SelectedCharacteristic != null && SelectedCharacteristic.Properties.Contains(BLECharacteristicPropertyTypes.Write));
        }

        private void _connectionStateChanged(object sender, BLEDevice e)
        {
            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
        }

        public async void ScanAsync()
        {
            await _gattConnection.StartScanAsync();
        }

        public async void ConnectAsync()
        {
            if (SelectedDevice != null)
            {
                await _gattConnection.StopScanAsync();
                await _gattConnection.ConnectAsync(SelectedDevice);
            }
        }

        public async void DisconnectAsync()
        {
            if (SelectedDevice != null)
            {
                await _gattConnection.DisconnectAsync(SelectedDevice);
            }
        }

        public override Task IsClosingAsync()
        {
            if (SelectedDevice != null && SelectedDevice.Connected)
            {
                _gattConnection.DisconnectAsync(SelectedDevice);
            }

            return base.IsClosingAsync();
        }

        public async void WriteCharacteristic()
        {
            if (await _gattConnection.WriteCharacteristic(SelectedDevice, SelectedService, SelectedCharacteristic, System.Text.ASCIIEncoding.ASCII.GetBytes(WriteText)))
            {
                Log.Insert(0, $"Wrote {SelectedCharacteristic.Name}");
            }
            else
            {
                Log.Insert(0, $"Could write {SelectedCharacteristic.Name}");
            }

        }

        public async void ReadCharacteristic()
        {
            var buffer = await _gattConnection.ReadCharacteristic(SelectedDevice, SelectedService, SelectedCharacteristic);
            if (buffer == null)
            {
                Log.Insert(0, "Could not read buffer");
            }
            else
            {
                Log.Insert(0, "Read Buffer -> " + System.Text.ASCIIEncoding.ASCII.GetString(buffer));
            }
        }

        public async void SubscribeCharacteristic()
        {
            if (await _gattConnection.SubscribeAsync(SelectedDevice, SelectedService, SelectedCharacteristic))
            {
                Log.Insert(0, $"Subscribed to {SelectedCharacteristic.Name}");
            }
            else
            {
                Log.Insert(0, $"Could not subscribe to {SelectedCharacteristic.Name}");
            }
        }

        public async void UnsubscribeCharacteristic()
        {
            if (await _gattConnection.UnsubscribeAsync(SelectedDevice, SelectedService, SelectedCharacteristic))
            {
                Log.Insert(0, $"Unsubscribed to {SelectedCharacteristic.Name}");
            }
            else
            {
                Log.Insert(0, $"Could not unsubscribe to {SelectedCharacteristic.Name}");
            }
        }

        public RelayCommand ScanCommand { get; }
        public RelayCommand ConnectCommand { get; }
        public RelayCommand DisconnectCommand { get; }

        public RelayCommand SubscribeCommand { get; }
        public RelayCommand UnsubscribeCommand { get; }
        public RelayCommand ReadCommand { get; }
        public RelayCommand WriteCommand { get; }

        public ObservableCollection<BLEDevice> DiscoveredDevices => _gattConnection.DiscoveredDevices;

        BLEDevice _selectedDevice;
        public BLEDevice SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                Set(ref _selectedDevice, value);
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
            }
        }

        BLEService _selectedService;
        public BLEService SelectedService
        {
            get => _selectedService;
            set => Set(ref _selectedService, value);
        }

        BLECharacteristic _selectedCharacteristic;
        public BLECharacteristic SelectedCharacteristic
        {
            get => _selectedCharacteristic;
            set
            {
                Set(ref _selectedCharacteristic, value);
                SubscribeCommand.RaiseCanExecuteChanged();
                UnsubscribeCommand.RaiseCanExecuteChanged();
                ReadCommand.RaiseCanExecuteChanged();
                WriteCommand.RaiseCanExecuteChanged();
            }
        }

        String _writeText;
        public String WriteText
        {
            get => _writeText;
            set => Set(ref _writeText, value);
        }


        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

    }
}
