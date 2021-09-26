using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;
using LagoVista.Core.UWP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;

namespace NuvIoT.HardwareClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort _port;
        private bool _isPaused;
        private IGATTConnection _gattConnection;
        private BLEDevice _currentDevice;

        public MainWindow()
        {
            InitializeComponent();

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

            DataContext = this;
            btnClose.IsEnabled = false;

            this.Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            btnBLEDisconnect.IsEnabled = false;

            _gattConnection = new GattConnection(new DispatcherServices(this.Dispatcher));
            _gattConnection.DeviceConnected += _gattConnection_DeviceConnected;
            _gattConnection.DeviceDisconnected += _gattConnection_DeviceDisconnected;
            _gattConnection.DeviceDiscovered += _gattConnection_DeviceDiscovered;
            _gattConnection.ReceiveConsoleOut += _gattConnection_ReceiveConsoleOut;
            _gattConnection.CharacteristicChanged += _gattConnection_CharacteristicChanged;
        }

        private void _gattConnection_CharacteristicChanged(object sender, BLECharacteristicsValue e)
        {
            var lines = e.Value.Split('\n');
            var lastLine = String.Empty;
            var timeStamp = DateTime.Now;

            foreach (var line in lines)
            {
                var formattedLine = $"{idx++:0000} - {e.Uid} - {timeStamp.Hour:00}:{timeStamp.Minute:00}.{timeStamp.Second:00}.{timeStamp.Millisecond:000} - {line.Trim()}";

                BLEOutput.Add(formattedLine);

                lastLine = formattedLine;
            }

            if (!_isPaused)
            {
                BLEOutputList.ScrollIntoView(lastLine);
            }
        }

        private void _gattConnection_ReceiveConsoleOut(object sender, string e)
        { 
            Dispatcher.Invoke(() => {
                BLEOutput.Add(e);
            });
        }

        private async void _gattConnection_DeviceDiscovered(object sender, BLEDevice e)
        {
            if (e.DeviceName.StartsWith("NuvIoT"))
            {
                await _gattConnection.StopScanAsync();
                await _gattConnection.ConnectAsync(e);                
            }
        }

        private void _gattConnection_DeviceDisconnected(object sender, BLEDevice e)
        {
            _currentDevice = null;
        }

        
        private void _gattConnection_DeviceConnected(object sender, BLEDevice e)
        {
            _currentDevice = e;
            var srvc = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Single(svc => svc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristicState = srvc.Characteristics.Single(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_STATE);
            var characteristicIoValue = srvc.Characteristics.Single(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IO_VALUE);
            var characteristicConsole = srvc.Characteristics.Single(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_CONSOLE);

            _gattConnection.SubscribeAsync(e, srvc, characteristicConsole);
            _gattConnection.SubscribeAsync(e, srvc, characteristicState);
            _gattConnection.SubscribeAsync(e, srvc, characteristicIoValue);
        }

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
                        //Debug.WriteLine(_buffer.ToString().Trim());
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

        protected async override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_currentDevice != null)
            {               
                await _gattConnection.DisconnectAsync(_currentDevice);
                _currentDevice = null;
            }

            await _gattConnection.StopScanAsync();

            if (_port != null)
            {
                _port.Close();
                _port.Dispose();
                _port = null;
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
            btnBLEStartWatcher.IsEnabled = false;
            btnBLEDisconnect.IsEnabled = true;
        }

        private async void btnBLEDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if(_currentDevice != null)
            {
                await _gattConnection.DisconnectAsync(_currentDevice);
                _currentDevice = null;
            }

            await _gattConnection.StopScanAsync();
            btnBLEStartWatcher.IsEnabled = true;
            btnBLEDisconnect.IsEnabled = false;
        }
    }
}
