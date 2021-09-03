using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels.DeviceAccess;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.Client.Core.ViewModels.DeviceSetup
{
    public class SensorDetailViewModel : DeviceViewModelBase
    {
        public const string SENSOR_ID = "sensorid";

        public override async Task InitAsync()
        {
            var service = BLEDevice.Services.First(svc => svc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.First(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IO_VALUE);

            await GattConnection.SubscribeAsync(BLEDevice, service, characteristics);
            await base.InitAsync();
        }

        public override async Task IsClosingAsync()
        {
            var service = BLEDevice.Services.First(svc => svc.Id == NuvIoTGATTProfile.SVC_UUID_NUVIOT);
            var characteristics = service.Characteristics.First(chr => chr.Id == NuvIoTGATTProfile.CHAR_UUID_IO_VALUE);
            await GattConnection.UnsubscribeAsync(BLEDevice, service, characteristics);
            await base.IsClosingAsync();
        }

        protected override void BLECharacteristicRead(BLECharacteristicsValue characteristic)
        {
            base.BLECharacteristicRead(characteristic);
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { Set(ref _port, value); }
        }

        private double _scaler;
        public double Scaler
        {
            get { return _scaler; }
            set { Set(ref _scaler, value); }
        }

        private string _sensorName;
        public string SensorName
        {
            get { return _sensorName; }
            set { Set(ref _sensorName, value); }
        }

        double _liveValue;
        public double LiveValue
        {
            get { return _liveValue; }
            set { Set(ref _liveValue, value); }
        }
    }
}
