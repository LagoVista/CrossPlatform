using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceAccess
{
    public class DeviceSerialPortAccessViewModel : AppViewModelBase
    {
        ISerialPort _currentPort;
        private CancellationTokenSource _listenCancelTokenSource = new CancellationTokenSource();

        public DeviceSerialPortAccessViewModel()
        {
            Responses = new ObservableCollection<string>();
        }

        async void ReadFromSerialPort(SerialPortInfo portInfo)
        {
            portInfo.BaudRate = 115200;
            portInfo.Parity = false;
            portInfo.DataBits = 8;

            _currentPort = DeviceManager.CreateSerialPort(portInfo);
            await _currentPort.OpenAsync();

            var _running = true;
            var buffer = new byte[1024];
            while (_running)
            {
                await _currentPort.ReadAsync(buffer, 0, buffer.Length, _listenCancelTokenSource.Token);
                var text = ASCIIEncoding.ASCII.GetString(buffer);
                var lines = text.Split('\n');
                foreach(var line in lines)
                {
                    Responses.Add(line.Trim());
                }
            }
        }

        public async override Task InitAsync()
        {
            SerialPorts = await DeviceManager.GetSerialPortsAsync();
           
            await base.InitAsync();
        }

        ObservableCollection<SerialPortInfo> _serialPorts;

        public ObservableCollection<SerialPortInfo> SerialPorts
        {
            get { return _serialPorts; }
            set { Set(ref _serialPorts, value); }
        }

        SerialPortInfo _selectedPort;
        public SerialPortInfo SelectedPort
        {
            get { return _selectedPort; }
            set
            {
                Set(ref _selectedPort, value);
                if (value != null)
                {
                    ReadFromSerialPort(value);
                }
            }
        }

        public ObservableCollection<string> Responses
        {
            get;
        }

        public override Task IsClosingAsync()
        {

            return base.IsClosingAsync();
        }
    }
}
