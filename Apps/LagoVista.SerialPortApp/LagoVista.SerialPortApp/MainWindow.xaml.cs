using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace LagoVista.SerialPortApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private SerialPort _port;
        private bool _isPaused = false;
        BLEDevice _device;

        /*BluetoothLEAdvertisementWatcher _watcher;
        BluetoothLEDevice _device;
        GattCharacteristic _stateCharacteristic;
        DevicePairingResult _pairingResult;
        List<GattCharacteristic> _subscribedCharacteristics = new List<GattCharacteristic>();*/

        System.Timers.Timer _timer;

        GattConnection _gattConnection;

        //SemaphoreSlim _deviceAccessLocker = new SemaphoreSlim(1, 1);
        #endregion

        public MainWindow()
        {
            PortNames.Add("-select-");
            BaudRates.Add("-select-");
            BaudRates.Add("9600");
            BaudRates.Add("19200");
            BaudRates.Add("38400");
            BaudRates.Add("57600");
            BaudRates.Add("115200");
            BaudRates.Add("230400");
            BaudRates.Add("250000");
            var portNames = SerialPort.GetPortNames();
            foreach (var portName in portNames)
            {
                if (portName == SelectedPortName)
                {
                    btnOpen.IsEnabled = SelectedBaudRate != "-select-";
                }
                PortNames.Add(portName);
            }

            _selectedPortName = DefaultConfig.Default.DefaultPortName;
            _selectedBaudRate = DefaultConfig.Default.DefaultBaudRate;

            InitializeComponent();

            DataContext = this;
            btnClose.IsEnabled = false;

            //_watcher = new BluetoothLEAdvertisementWatcher()
            //{
            //    ScanningMode = BluetoothLEScanningMode.Active
            //};

            //_watcher.Received += Watcher_Received;

            //_timer = new System.Timers.Timer();
            //_timer.Interval = 1000;
            //_timer.Elapsed += _timer_Elapsed;
            //_timer.Start();

            this.Loaded += MainWindow_Loaded;
        }

        public const string SVC_UUID_NUVIOT = "d804b639-6ce7-4e80-9f8a-ce0f699085eb";
        public const string CHAR_UUID_STATE = "d804b639-6ce7-5e81-9f8a-ce0f699085eb";

        //private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    await _deviceAccessLocker.WaitAsync();
        //    if (_device != null && _stateCharacteristic != null)
        //    {
        //        var pingBuffer = System.Text.ASCIIEncoding.ASCII.GetBytes("ping");
        //        try
        //        {
        //            GattCommunicationStatus statusResult = await _stateCharacteristic.WriteValueAsync(pingBuffer.AsBuffer());
        //        }
        //        catch (Exception)
        //        {
        //            await DisconnectBLEWithoutLock();
        //        }
        //    }

        //    _deviceAccessLocker.Release();
        //}

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            btnBLEDisconnect.IsEnabled = false;

            _gattConnection = new GattConnection(this.Dispatcher);
            _gattConnection.DeviceConnected += _gattConnection_DeviceConnected;
            _gattConnection.DeviceDisconnected += _gattConnection_DeviceDisconnected;
            _gattConnection.DeviceDiscovered += _gattConnection_DeviceDiscovered;
            _gattConnection.ReceiveConsoleOut += _gattConnection_ReceiveConsoleOut;
            _gattConnection.CharacteristicChanged += _gattConnection_CharacteristicChanged;
        }

        private void _gattConnection_CharacteristicChanged(object sender, BLECharacteristicsValue e)
        {
            throw new NotImplementedException();
        }

        private void _gattConnection_ReceiveConsoleOut(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private async void _gattConnection_DeviceDiscovered(object sender, BLEDevice e)
        {
            if (e.DeviceName.StartsWith("NuvIoT"))
                await _gattConnection.ConnectAsync(e);

            throw new NotImplementedException();
        }

        private void _gattConnection_DeviceDisconnected(object sender, BLEDevice e)
        {
            throw new NotImplementedException();
        }

        private void _gattConnection_DeviceConnected(object sender, BLEDevice e)
        {
            throw new NotImplementedException();
        }

        /*
#region BLE
private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
{
   if (args.Advertisement.LocalName.Contains("NuvIoT"))
   {
       _watcher.Stop();
       await _deviceAccessLocker.WaitAsync();
       try
       {
           _device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
           if (_device != null)
           {
               _device.ConnectionStatusChanged += Device_ConnectionStatusChanged;

               var gatt = await _device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
               _device.DeviceInformation.Pairing.Custom.PairingRequested += Custom_PairingRequested;
               _pairingResult = await _device.DeviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly);
               Debug.WriteLine($"BLE NuvIoT Device => {_device.Name} - Pairing Result {_pairingResult.Status}");

               if (gatt.Status == Windows.Devices.Bluetooth.GenericAttributeProfile.GattCommunicationStatus.Success)
               {
                   foreach (var srvc in gatt.Services)
                   {
                       Debug.WriteLine("Top Level Service: " + srvc.Uuid);
                       var charactersistics = await srvc.GetCharacteristicsAsync();                                
                       foreach (var characteristic in charactersistics.Characteristics)
                       {
                           if (characteristic.Uuid == Guid.Parse(CHAR_UUID_STATE))
                               _stateCharacteristic = characteristic;

                           Debug.WriteLine("    Processing Characteristic: " + characteristic.Uuid);

                           var result = await characteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
                           if (result.Status != GattCommunicationStatus.Success)
                           {
                               Debug.WriteLine($"Could not get descriptor: { result.ProtocolError}");
                               return;
                           }
                           else
                           {
                               if (characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                               {
                                   Debug.WriteLine("    Supports Notify: " + characteristic.Uuid + " " + characteristic.CharacteristicProperties);

                                   characteristic.ProtectionLevel = GattProtectionLevel.Plain;

                                   var descriptor = await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
                                   if (descriptor.Status != GattCommunicationStatus.Success)
                                   {
                                       Debug.WriteLine("  Could not Supports Notify: " + characteristic.Uuid + " " + characteristic.CharacteristicProperties);
                                   }
                                   else
                                   {
                                       GattCommunicationStatus statusResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                                       if (statusResult == GattCommunicationStatus.Success)
                                       {
                                           characteristic.ValueChanged += Characteristic_ValueChanged;
                                           _subscribedCharacteristics.Add(characteristic);
                                       }
                                       else
                                       {
                                           Debug.WriteLine("Could not subscribe to indicate.");
                                       }
                                   }
                               }
                               else
                               {
                                   Debug.WriteLine("    Does not supports notify: " + characteristic.Uuid + " " + characteristic.CharacteristicProperties);
                               }
                           }
                       }
                   }

               }
               else
               {
                   Debug.WriteLine("Could not connect.");
                   _watcher.Start();
               }

           }
       }
       finally
       {
           _deviceAccessLocker.Release();
       }
   }
   else
   {
       if (!String.IsNullOrEmpty(args.Advertisement.LocalName))
       {
           Debug.WriteLine("BLE Non-NuvIoT Device => " + args.Advertisement.LocalName);
       }
   }
}

private void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
{
   args.Accept();
}

private async Task DisconnectBLEWithoutLock()
{
   if (_device != null)
   {
       foreach (var characteristic in _subscribedCharacteristics)
       {
           try
           {
               characteristic.ValueChanged -= Characteristic_ValueChanged;
               var writeResult = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
           }
           catch (Exception ex)
           {

           }
           finally
           {

           }
       }

       var services = await _device.GetGattServicesAsync();

       foreach (var srvc in services.Services)
       {
           try
           {
               srvc.Dispose();
           }
           catch (Exception ex)
           {
               Debug.WriteLine($"!! Exception in GetGattServicesAsync to disconnect: {ex.Message} !!");
           }
       }

       _device.ConnectionStatusChanged -= Device_ConnectionStatusChanged;
       _device.Dispose();
       _device = null;
   }
}

private async void DisconnectBLE()
{
   await _deviceAccessLocker.WaitAsync();
   if (_device != null)
   {
       await DisconnectBLEWithoutLock();
   }

   _deviceAccessLocker.Release();
}

private async void Characteristic_ValueChanged(Windows.Devices.Bluetooth.GenericAttributeProfile.GattCharacteristic sender, Windows.Devices.Bluetooth.GenericAttributeProfile.GattValueChangedEventArgs args)
{
   var value = await sender.ReadValueAsync();
   var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(value.Value);
   var output = dataReader.ReadString(value.Value.Length);

   var lines = output.Split('\n');
   var lastLine = String.Empty;
   var timeStamp = DateTime.Now;

   Dispatcher.Invoke(() =>
   {
       foreach (var line in lines)
       {
           var formattedLine = $"{idx++:0000} - {sender.Uuid} - {timeStamp.Hour:00}:{timeStamp.Minute:00}.{timeStamp.Second:00}.{timeStamp.Millisecond:000} - {line.Trim()}";

           BLEOutput.Add(formattedLine);

           lastLine = formattedLine;
       }

       if (!_isPaused)
       {
           BLEOutputList.ScrollIntoView(lastLine);
       }
   });
}

private async void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
{
   if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
   {
       await DisconnectBLEWithoutLock();
       Dispatcher.Invoke(() =>
       {
           rectBTConnected.Fill = new SolidColorBrush(Colors.Red);
           btnBLEStartWatcher.IsEnabled = true;
           btnBLEDisconnect.IsEnabled = false;
       });
   }
   else if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
   {
       Dispatcher.Invoke(() =>
       {
           rectBTConnected.Fill = new SolidColorBrush(Colors.Green);
           btnBLEDisconnect.IsEnabled = true;
       });
   }
   Debug.WriteLine($"BLE Connection Status: {sender.ConnectionStatus}");
}*/
#endregion

        #region Serial Port
        private void OpenPort(string portName, int baudRate)
        {
            btnOpen.IsEnabled = false;

            var port = new SerialPort(portName, baudRate);
            port.Open();
            if (port.IsOpen)
            {
                btnClose.IsEnabled = true;
                btnOpen.IsEnabled = false;
                _port = port;
                _port.DataReceived += _port_DataReceived;
            }
            else
            {
                btnClose.IsEnabled = false;
                btnOpen.IsEnabled = true;

            }
        }

        private int idx = 1;

        StringBuilder _buffer = new StringBuilder();

        private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var buffer = new byte[1024];

            var read = _port.Read(buffer, 0, Math.Min(1024, _port.BytesToRead));

            var allText = System.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, read);
            var lines = allText.Split('\n');
            var lastLine = String.Empty;
            var timeStamp = DateTime.Now;

            Dispatcher.Invoke(() =>
            {
                foreach (var ch in allText)
                {
                    if (ch == '\n')
                    {
                        var formattedLine = $"{idx++:0000} - {timeStamp.Hour:00}:{timeStamp.Minute:00}.{timeStamp.Second:00}.{timeStamp.Millisecond:000} - {_buffer.ToString().Trim()}";
                        ConsoleOutput.Add(formattedLine);
                        Debug.WriteLine(_buffer.ToString().Trim());
                        lastLine = formattedLine;
                        _buffer.Clear();
                    }
                    else
                    {
                        _buffer.Append(ch);
                    }
                }

                if (!_isPaused && !String.IsNullOrEmpty(lastLine))
                {
                    ConsoleOutputList.ScrollIntoView(lastLine);
                }
            });
        }
        #endregion

        #region Properties
        private string _selectedPortName = "-select-";
        public string SelectedPortName
        {
            get => _selectedPortName;
            set
            {
                _selectedPortName = value;
                DefaultConfig.Default.DefaultPortName = value;
                DefaultConfig.Default.Save();

                btnOpen.IsEnabled = value != "-select" && _selectedBaudRate != "-select-";
            }
        }

        private string _selectedBaudRate = "-select-";
        public string SelectedBaudRate
        {
            get => _selectedBaudRate;
            set
            {
                _selectedBaudRate = value;
                DefaultConfig.Default.DefaultBaudRate = value;
                DefaultConfig.Default.Save();


                btnOpen.IsEnabled = value != "-select" && _selectedPortName != "-select-";
            }
        }

        private bool _sendCR;
        private bool _sendLF;

        public bool SendCR
        {
            get => _sendCR;
            set
            {
                _sendCR = value;
                DefaultConfig.Default.DefaultSendCR = value;
                DefaultConfig.Default.Save();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SendCR)));
            }
        }

        public bool SendLF
        {
            get => _sendLF;
            set
            {
                _sendLF = value;
                DefaultConfig.Default.DefaultSendLF = value;
                DefaultConfig.Default.Save();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SendLF)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> ConsoleOutput { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> BLEOutput { get; } = new ObservableCollection<string>();

        public ObservableCollection<string> PortNames { get; } = new ObservableCollection<string>();

        public List<string> BaudRates { get; } = new List<string>();
        #endregion

        #region Event Handlers
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenPort(SelectedPortName, int.Parse(SelectedBaudRate));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_port != null)
            {
                _port.DataReceived -= _port_DataReceived;
                _port.Close();
                _port.Dispose();
                _port = null;
                btnClose.IsEnabled = false;
                btnOpen.IsEnabled = true;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_port != null)
            {
                _port.Close();
                _port.Dispose();
            }
        }

        private void btnPauseResume_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused)
            {
                btnPauseResume.Content = "Pause";
                _isPaused = false;
            }
            else
            {
                btnPauseResume.Content = "Resume";
                _isPaused = true;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Clear();
            BLEOutput.Clear();
        }

        private void SendCommand(string command)
        {
            if (_port != null && _port.IsOpen)
            {
                _port.Write($"{command}\n");
            }
        }

        private void btnSendRun_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("run");
        }

        private void btnSendRestart_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("restart");
        }

        private void btnSendPause_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("pause");
        }

        private void btnSendExit_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("exit");
        }

        private void txtCommandText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == System.Windows.Input.Key.Return) && _port != null && _port.IsOpen)
            {
                var commandText = txtCommandText.Text;

                if (SendCR)
                {
                    commandText += "\r";
                }

                if (SendLF)
                {
                    commandText += "\n";
                }

                if (!String.IsNullOrEmpty(commandText))
                {
                    _port.Write(commandText);
                    commandText = String.Empty;
                }

                txtCommandText.Text = String.Empty;
            }
        }
        #endregion

        private async void btnBLEStartWatcher_Click(object sender, RoutedEventArgs e)
        {
            await _gattConnection.StartScanAsync();
            //_watcher.Start();
            btnBLEStartWatcher.IsEnabled = false;
            btnBLEDisconnect.IsEnabled = true;
        }

        private async void btnBLEDisconnect_Click(object sender, RoutedEventArgs e)
        {
            await _gattConnection.StopScanAsync();
            //DisconnectBLE();
            //_watcher.Stop();
            btnBLEStartWatcher.IsEnabled = true;
            btnBLEDisconnect.IsEnabled = false;
        }
    }
}
