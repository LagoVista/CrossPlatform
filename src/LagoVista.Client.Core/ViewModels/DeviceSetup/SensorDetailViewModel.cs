using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using LagoVista.Core.Commanding;
using LagoVista.IoT.DeviceManagement.Models;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceSetup
{
    public class SensorDetailViewModel : DeviceViewModelBase
    {
        public const string SENSOR_ID = "sensorid";
        public const string SENSOR = "sensor";

        Sensor _sensor;

        public SensorDetailViewModel()
        {
            ReadCommand = new RelayCommand(Read);
            WriteCommand = new RelayCommand(Write);
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();

            if (LaunchArgs.Parameters.ContainsKey(SENSOR))
            {
                Sensor = LaunchArgs.GetParam<Sensor>(SENSOR);
            }

            var service = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Find(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.First(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IO_VALUE);

            await GattConnection.SubscribeAsync(BLEDevice, service, characteristics);
        }

        public override async Task IsClosingAsync()
        {
            var service = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Find(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.First(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IO_VALUE);
            await GattConnection.UnsubscribeAsync(BLEDevice, service, characteristics);
            await base.IsClosingAsync();
        }

        protected override void BLECharacteristicRead(BLECharacteristicsValue characteristic)
        {
            base.BLECharacteristicRead(characteristic);
        }

        public async void Read()
        {
            var service = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Find(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.First(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IOCONFIG);
            await GattConnection.WriteCharacteristic(BLEDevice, service, characteristics, "readadc=1;");
            var result = await GattConnection.ReadCharacteristicAsync(BLEDevice, service, characteristics);
            Debug.WriteLine(System.Text.ASCIIEncoding.ASCII.GetString(result));
        }

        public async void Write()
        {
            var service = NuvIoTGATTProfile.GetNuvIoTGATT().Services.Find(srvc => srvc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.First(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IOCONFIG);
            await GattConnection.WriteCharacteristic(BLEDevice, service, characteristics, "writeadc=1,1,1.1,2.2,3.3;");
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { Set(ref _port, value); }
        }

        public Sensor Sensor
        {
            get => _sensor;
            set{Set(ref _sensor, value);}
        }

        double _liveValue;
        public double LiveValue
        {
            get { return _liveValue; }
            set { Set(ref _liveValue, value); }
        }

        public RelayCommand  ReadCommand { get; }
        public RelayCommand WriteCommand { get; }
    }
}
